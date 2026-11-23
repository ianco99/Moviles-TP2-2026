package com.example.kuznickilogger;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.util.Log;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class KuznickiLoggerManager {
    private static final String TAG = "KuznickiLog";
    private static final String LOGS_FOLDER = "logs";
    private static final String SESSION_FILE_NAME = "session.log";
    private static final String CONTINUATION_INDENT = "    ";
    private static final String LINE_BREAK = "\n";
    private static final char ENTRY_SEPARATOR = '\u001E';
    private static final long MAX_SESSION_BYTES = 4L * 1024L * 1024L;

    private static final KuznickiLoggerManager instance = new KuznickiLoggerManager();
    public static KuznickiLoggerManager getInstance() {return instance;}

    private final SimpleDateFormat timeFormat = new SimpleDateFormat("HH:mm:ss.SSS", Locale.US);
    private Context context;
    private File sessionFile;
    private BufferedWriter sessionWriter;
    private long sessionBytes;

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
        if (this.context != null)
            return;

        this.context = context.getApplicationContext();

        File folder = new File(this.context.getFilesDir(), LOGS_FOLDER);
        if (!folder.exists() && !folder.mkdirs())
            Log.w(TAG, "Could not create the logs folder " + folder.getAbsolutePath());

        sessionFile = new File(folder, SESSION_FILE_NAME);
        openSession();
    }

    public synchronized void sendLog(int priority, long timeMillis, String message, String stackTrace)
    {
        Log.println(priority, TAG, message);
        appendToSession(formatLine(priority, timeMillis, message, stackTrace));
    }

    public synchronized String getLogs()
    {
        if (sessionFile == null || !sessionFile.exists())
            return "";

        flushSession();
        StringBuilder builder = new StringBuilder();

        try (BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(sessionFile), StandardCharsets.UTF_8)))
        {
            String line;
            while ((line = reader.readLine()) != null)
            {
                if (builder.length() > 0)
                    builder.append(line.startsWith(CONTINUATION_INDENT) ? LINE_BREAK : String.valueOf(ENTRY_SEPARATOR));

                builder.append(line);
            }
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not read the session log " + sessionFile.getAbsolutePath(), exception);
        }

        return builder.toString();
    }

    public void clearLogs(Activity activity, ClearLogsListener listener)
    {
        activity.runOnUiThread(() -> showClearConfirmation(activity, listener));
    }

    private void showClearConfirmation(Activity activity, ClearLogsListener listener)
    {
        if (activity.isFinishing())
            return;

        new AlertDialog.Builder(activity, android.R.style.Theme_DeviceDefault_Dialog_Alert)
                .setTitle(R.string.kuznicki_logger_clear_title)
                .setMessage(R.string.kuznicki_logger_clear_message)
                .setPositiveButton(R.string.kuznicki_logger_clear_confirm, (dialog, which) -> onClearConfirmed(listener))
                .setNegativeButton(R.string.kuznicki_logger_clear_cancel, null)
                .show();
    }

    private void onClearConfirmed(ClearLogsListener listener)
    {
        clearSession();
        listener.onLogsCleared();
    }

    private synchronized void clearSession()
    {
        openSession();
    }

    private String formatLine(int priority, long timeMillis, String message, String stackTrace)
    {
        StringBuilder builder = new StringBuilder();
        builder.append(timeFormat.format(new Date(timeMillis)));
        builder.append(' ');
        builder.append(priorityLetter(priority));
        builder.append(' ');

        String[] messageLines = message.split(LINE_BREAK, -1);
        builder.append(messageLines[0]);
        for (int i = 1; i < messageLines.length; i++)
            appendContinuation(builder, messageLines[i]);

        if (stackTrace == null || stackTrace.trim().isEmpty())
            return builder.toString();

        for (String stackLine : stackTrace.trim().split(LINE_BREAK))
            appendContinuation(builder, stackLine);

        return builder.toString();
    }

    private void appendContinuation(StringBuilder builder, String line)
    {
        builder.append('\n');
        builder.append(CONTINUATION_INDENT);
        builder.append(line);
    }

    private char priorityLetter(int priority)
    {
        switch (priority)
        {
            case Log.VERBOSE: return 'V';
            case Log.DEBUG: return 'D';
            case Log.INFO: return 'I';
            case Log.WARN: return 'W';
            case Log.ERROR: return 'E';
            case Log.ASSERT: return 'A';
            default: return '?';
        }
    }

    private void openSession()
    {
        closeSession();
        sessionBytes = 0;

        if (sessionFile == null)
            return;

        try
        {
            sessionWriter = new BufferedWriter(new OutputStreamWriter(new FileOutputStream(sessionFile, false), StandardCharsets.UTF_8));
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not open the session log " + sessionFile.getAbsolutePath(), exception);
            sessionWriter = null;
        }
    }

    private void appendToSession(String text)
    {
        if (sessionWriter == null)
            return;

        if (sessionBytes > MAX_SESSION_BYTES)
        {
            openSession();
            appendToSession("Session log passed " + MAX_SESSION_BYTES + " bytes and was restarted");
        }

        try
        {
            sessionWriter.write(text);
            sessionWriter.write('\n');
            sessionWriter.flush();
            sessionBytes += text.length() + 1;
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not write to the session log", exception);
        }
    }

    private void flushSession()
    {
        if (sessionWriter == null)
            return;

        try
        {
            sessionWriter.flush();
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not flush the session log", exception);
        }
    }

    private void closeSession()
    {
        if (sessionWriter == null)
            return;

        try
        {
            sessionWriter.close();
        }
        catch (IOException exception)
        {
            Log.e(TAG, "Could not close the session log", exception);
        }

        sessionWriter = null;
    }
}
