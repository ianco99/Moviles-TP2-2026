package com.example.kuznickilogger;

import android.annotation.TargetApi;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Context;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Log;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

final class LogExporter {
    private static final String TAG = "KuznickiLog";
    private static final String EXPORT_FOLDER = "GunJump";
    private static final String FILE_PREFIX = "GunJump_logs_";
    private static final String FILE_EXTENSION = ".txt";
    private static final String MIME_TYPE = "text/plain";
    private static final String UNITY_SECTION = "== Unity ==";
    private static final String LOGCAT_SECTION = "== Logcat ==";

    private final Context context;

    LogExporter(Context context)
    {
        this.context = context;
    }

    String export(File sessionFile)
    {
        String fileName = FILE_PREFIX + new SimpleDateFormat("yyyyMMdd_HHmmss", Locale.US).format(new Date()) + FILE_EXTENSION;

        try
        {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q)
                return exportToPublicDownloads(fileName, sessionFile);

            return exportToAppDownloads(fileName, sessionFile);
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not export the logs to " + fileName, exception);
            return "";
        }
    }

    @TargetApi(Build.VERSION_CODES.Q)
    private String exportToPublicDownloads(String fileName, File sessionFile) throws IOException
    {
        String relativeFolder = Environment.DIRECTORY_DOWNLOADS + "/" + EXPORT_FOLDER;
        ContentResolver resolver = context.getContentResolver();

        ContentValues values = new ContentValues();
        values.put(MediaStore.MediaColumns.DISPLAY_NAME, fileName);
        values.put(MediaStore.MediaColumns.MIME_TYPE, MIME_TYPE);
        values.put(MediaStore.MediaColumns.RELATIVE_PATH, relativeFolder);
        values.put(MediaStore.MediaColumns.IS_PENDING, 1);

        Uri uri = resolver.insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI, values);
        if (uri == null)
            throw new IOException("MediaStore did not create " + fileName);

        try (OutputStream output = resolver.openOutputStream(uri))
        {
            if (output == null)
                throw new IOException("MediaStore did not open " + uri);

            writeReport(output, sessionFile);
        }
        catch (IOException exception)
        {
            resolver.delete(uri, null, null);
            throw exception;
        }

        values.clear();
        values.put(MediaStore.MediaColumns.IS_PENDING, 0);
        resolver.update(uri, values, null, null);

        return relativeFolder + "/" + fileName;
    }

    private String exportToAppDownloads(String fileName, File sessionFile) throws IOException
    {
        File folder = context.getExternalFilesDir(Environment.DIRECTORY_DOWNLOADS);
        if (folder == null)
            throw new IOException("External storage is not available");

        File file = new File(folder, fileName);
        try (OutputStream output = new FileOutputStream(file))
        {
            writeReport(output, sessionFile);
        }

        return file.getAbsolutePath();
    }

    private void writeReport(OutputStream output, File sessionFile) throws IOException
    {
        Writer writer = new BufferedWriter(new OutputStreamWriter(output, StandardCharsets.UTF_8));

        writeHeader(writer);

        writer.write('\n');
        writer.write(UNITY_SECTION);
        writer.write('\n');
        copySession(sessionFile, writer);

        writer.write('\n');
        writer.write(LOGCAT_SECTION);
        writer.write('\n');
        copyProcessLogcat(writer);

        writer.flush();
    }

    private void writeHeader(Writer writer) throws IOException
    {
        writer.write("Package: " + context.getPackageName() + '\n');
        writer.write("Version: " + versionName() + '\n');
        writer.write("Device: " + Build.MANUFACTURER + ' ' + Build.MODEL + '\n');
        writer.write("Android: " + Build.VERSION.RELEASE + " (API " + Build.VERSION.SDK_INT + ")\n");
        writer.write("Exported: " + new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.US).format(new Date()) + '\n');
    }

    private String versionName()
    {
        try
        {
            return context.getPackageManager().getPackageInfo(context.getPackageName(), 0).versionName;
        }
        catch (PackageManager.NameNotFoundException exception)
        {
            return "unknown";
        }
    }

    private void copySession(File sessionFile, Writer writer) throws IOException
    {
        if (sessionFile == null || !sessionFile.exists())
        {
            writer.write("No session log\n");
            return;
        }

        try (InputStream input = new FileInputStream(sessionFile))
        {
            copyLines(input, writer);
        }
    }

    private void copyProcessLogcat(Writer writer) throws IOException
    {
        String processFilter = "--pid=" + android.os.Process.myPid();
        Process logcat;

        try
        {
            logcat = new ProcessBuilder("logcat", "-d", "-v", "time", processFilter).redirectErrorStream(true).start();
        }
        catch (IOException exception)
        {
            writer.write("Logcat is not available: " + exception.getMessage() + '\n');
            return;
        }

        try (InputStream input = logcat.getInputStream())
        {
            copyLines(input, writer);
        }
        finally
        {
            logcat.destroy();
        }
    }

    private void copyLines(InputStream input, Writer writer) throws IOException
    {
        BufferedReader reader = new BufferedReader(new InputStreamReader(input, StandardCharsets.UTF_8));
        String line;

        while ((line = reader.readLine()) != null)
        {
            writer.write(line);
            writer.write('\n');
        }
    }
}
