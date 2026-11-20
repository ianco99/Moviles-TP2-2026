package com.example.kuznickilogger;

import android.content.Context;
import android.util.Log;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.ArrayDeque;
import java.util.Date;
import java.util.Iterator;
import java.util.Locale;

public class KuznickiLoggerManager {
    private static final String TAG = "KuznickiLog";
    private static final String LOGS_FOLDER = "logs";
    private static final String SESSION_FILE_NAME = "session.log";
    private static final String STACK_TRACE_INDENT = "    ";
    private static final char ENTRY_SEPARATOR = '\u001E';
    private static final int MAX_BUFFERED_LINES = 1000;
    private static final long MAX_SESSION_BYTES = 4L * 1024L * 1024L;

    private static final KuznickiLoggerManager instance = new KuznickiLoggerManager();
    public static KuznickiLoggerManager getInstance() {return instance;}

    private final ArrayDeque<LogLine> bufferedLines = new ArrayDeque<>();
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

    public synchronized void log(int priority, long timeMillis, String message, String stackTrace)
    {
        Log.println(priority, TAG, message);

        String text = formatLine(priority, timeMillis, message, stackTrace);
        bufferedLines.addLast(new LogLine(priority, text));
        while (bufferedLines.size() > MAX_BUFFERED_LINES)
            bufferedLines.removeFirst();

        appendToSession(text);
    }

    public synchronized String getLogs(int minPriority, int maxEntries)
    {
        int matches = 0;
        LogLine firstMatch = null;
        Iterator<LogLine> newestFirst = bufferedLines.descendingIterator();

        while (newestFirst.hasNext() && matches < maxEntries)
        {
            LogLine logLine = newestFirst.next();
            if (logLine.priority >= minPriority)
            {
                matches++;
                firstMatch = logLine;
            }
        }

        StringBuilder builder = new StringBuilder();
        boolean isCollecting = false;

        for (LogLine logLine : bufferedLines)
        {
            if (logLine == firstMatch)
                isCollecting = true;

            if (!isCollecting || logLine.priority < minPriority)
                continue;

            if (builder.length() > 0)
                builder.append(ENTRY_SEPARATOR);

            builder.append(logLine.text);
        }

        return builder.toString();
    }

    public synchronized int getLogCount()
    {
        return bufferedLines.size();
    }

    public synchronized void clear()
    {
        bufferedLines.clear();
        openSession();
    }

    public synchronized String exportLogs()
    {
        if (context == null)
        {
            Log.e(TAG, "exportLogs was called before init");
            return "";
        }

        flushSession();
        return new LogExporter(context).export(sessionFile);
    }

    private String formatLine(int priority, long timeMillis, String message, String stackTrace)
    {
        StringBuilder builder = new StringBuilder();
        builder.append(timeFormat.format(new Date(timeMillis)));
        builder.append(' ');
        builder.append(priorityLetter(priority));
        builder.append(' ');
        builder.append(message);

        if (stackTrace == null || stackTrace.trim().isEmpty())
            return builder.toString();

        for (String stackLine : stackTrace.trim().split("\n"))
        {
            builder.append('\n');
            builder.append(STACK_TRACE_INDENT);
            builder.append(stackLine);
        }

        return builder.toString();
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

    private static final class LogLine
    {
        final int priority;
        final String text;

        LogLine(int priority, String text)
        {
            this.priority = priority;
            this.text = text;
        }
    }
}
