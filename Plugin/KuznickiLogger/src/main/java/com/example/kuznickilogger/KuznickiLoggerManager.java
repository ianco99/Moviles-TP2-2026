package com.example.kuznickilogger;

import android.util.Log;

public class KuznickiLoggerManager {
    private static final KuznickiLoggerManager instance = new KuznickiLoggerManager();
    public static KuznickiLoggerManager getInstance() {return instance;}

    public long getTime()
    {
    return System.currentTimeMillis();
    }


    public void sendLong(String msg)
    {
        Log.i("KuznickiLog", msg);
    }
}
