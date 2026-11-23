package com.example.kuznickilogger;

import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.os.VibratorManager;

public final class KuznickiVibrator {
    private static final int MAX_AMPLITUDE = 255;

    private final Vibrator vibrator;

    public KuznickiVibrator(Context context) {
        Context application = context.getApplicationContext();

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            VibratorManager manager = (VibratorManager) application.getSystemService(Context.VIBRATOR_MANAGER_SERVICE);
            vibrator = manager != null ? manager.getDefaultVibrator() : null;
        } else {
            vibrator = (Vibrator) application.getSystemService(Context.VIBRATOR_SERVICE);
        }
    }

    public boolean hasVibrator() {
        return vibrator != null && vibrator.hasVibrator();
    }

    public void vibrate(long milliseconds, int amplitude) {
        if (!hasVibrator() || milliseconds <= 0) {
            return;
        }

        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            vibrator.vibrate(milliseconds);
            return;
        }

        int strength = vibrator.hasAmplitudeControl()
                ? Math.max(1, Math.min(MAX_AMPLITUDE, amplitude))
                : VibrationEffect.DEFAULT_AMPLITUDE;
        vibrator.vibrate(VibrationEffect.createOneShot(milliseconds, strength));
    }

    public void cancel() {
        if (vibrator != null) {
            vibrator.cancel();
        }
    }
}
