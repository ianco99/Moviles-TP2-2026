package com.example.kuznickilogger;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.util.Log;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class KuznickiLoggerManager {
    private static final String TAG = "KuznickiLog";
    private static final String LOGS_FILE_NAME = "logs.txt";

    private static final KuznickiLoggerManager instance = new KuznickiLoggerManager();
    public static KuznickiLoggerManager getInstance() {return instance;}

    private final SimpleDateFormat timeFormat = new SimpleDateFormat("HH:mm:ss.SSS", Locale.US);
    private File logsFile;

    public long getTime()
    {
    return System.currentTimeMillis();
    }


    public void sendLong(String msg)
    {
        Log.i(TAG, msg);
    }

    public synchronized void init(Context context)
    {
        logsFile = new File(context.getFilesDir(), LOGS_FILE_NAME);
        deleteLogs();
    }

    public synchronized void sendLog(String type, String message)
    {
        String line = timeFormat.format(new Date()) + " [" + type + "] " + message;
        Log.i(TAG, line);

        if (logsFile == null)
            return;

        try (Writer writer = new OutputStreamWriter(new FileOutputStream(logsFile, true), StandardCharsets.UTF_8))
        {
            writer.write(line);
            writer.write('\n');
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not write to " + logsFile.getAbsolutePath(), exception);
        }
    }

    public synchronized String getLogs()
    {
        if (logsFile == null || !logsFile.exists())
            return "";

        StringBuilder builder = new StringBuilder();

        try (BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(logsFile), StandardCharsets.UTF_8)))
        {
            String line;
            while ((line = reader.readLine()) != null)
            {
                builder.append(line);
                builder.append('\n');
            }
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not read " + logsFile.getAbsolutePath(), exception);
        }

        return builder.toString();
    }

    public void clearLogs(Activity activity)
    {
        activity.runOnUiThread(() -> new AlertDialog.Builder(activity, android.R.style.Theme_DeviceDefault_Dialog_Alert)
                .setTitle(R.string.kuznicki_logger_clear_title)
                .setMessage(R.string.kuznicki_logger_clear_message)
                .setPositiveButton(R.string.kuznicki_logger_clear_confirm, (dialog, which) -> deleteLogs())
                .setNegativeButton(R.string.kuznicki_logger_clear_cancel, null)
                .show());
    }

    private synchronized void deleteLogs()
    {
        if (logsFile != null && logsFile.exists() && !logsFile.delete())
            Log.w(TAG, "Could not delete " + logsFile.getAbsolutePath());
    }
}
