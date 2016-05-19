package com.example.alavis.unityusbactivity;

/**
 * Created by alavis on 5/13/2016.
 */

import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.os.Bundle;
import android.os.Handler;
import android.os.IBinder;
import android.os.Message;
import android.widget.Toast;

import com.felhr.usbserial.UsbSerialDevice;
import com.unity3d.player.UnityPlayerActivity;

import java.io.IOException;
import java.lang.ref.WeakReference;
import java.util.Arrays;
import java.util.LinkedList;
import java.util.Queue;
import java.util.Set;

public class UnityUSBActivity extends UnityPlayerActivity {
    private final BroadcastReceiver mUsbReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (intent.getAction() == UsbService.ACTION_USB_PERMISSION_GRANTED) // USB PERMISSION GRANTED
            {
                Toast.makeText(context, "USB Ready", Toast.LENGTH_SHORT).show();
                granted = true;
            }
            else if (intent.getAction() == UsbService.ACTION_USB_PERMISSION_NOT_GRANTED) // USB PERMISSION NOT GRANTED
                Toast.makeText(context, "USB Permission not granted", Toast.LENGTH_SHORT).show();
            else if (intent.getAction() == UsbService.ACTION_NO_USB) // NO USB CONNECTED
                Toast.makeText(context, "No USB connected", Toast.LENGTH_SHORT).show();
            else if (intent.getAction() == UsbService.ACTION_USB_DISCONNECTED) // USB DISCONNECTED
                Toast.makeText(context, "USB disconnected", Toast.LENGTH_SHORT).show();
            else if (intent.getAction() == UsbService.ACTION_USB_NOT_SUPPORTED) // USB NOT SUPPORTED
                Toast.makeText(context, "USB device not supported", Toast.LENGTH_SHORT).show();
        }
    };
    private UsbService usbService;
    private MyHandler mHandler;
    private volatile boolean granted = false;
    public boolean isUSBAccessGranted()
    {
        return granted;
    }

    public UsbSerialDevice getSerialPort() throws IOException {
        if (granted)
            return usbService.serialPort;
        else
            throw new IOException("Access to USB not granted.");
    }

    public UsbService getService() throws IOException {
        if (granted)
            return usbService;
        else
            throw new IOException("Access to USB not granted.");
    }
    private final ServiceConnection usbConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName arg0, IBinder arg1) {
            usbService = ((UsbService.UsbBinder) arg1).getService();
            usbService.setHandler(mHandler);
        }

        @Override
        public void onServiceDisconnected(ComponentName arg0) {
            usbService = null;
        }
    };
    @Override
    public void onResume() {
        super.onResume();
        setFilters();  // Start listening notifications from UsbService
        startService(UsbService.class, usbConnection, null); // Start UsbService(if it was not started before) and Bind it
    }

    @Override
    public void onPause() {
        super.onPause();
        unregisterReceiver(mUsbReceiver);
        unbindService(usbConnection);
    }
    private void startService(Class<?> service, ServiceConnection serviceConnection, Bundle extras) {
        if (!UsbService.SERVICE_CONNECTED) {
            Intent startService = new Intent(this, service);
            if (extras != null && !extras.isEmpty()) {
                Set<String> keys = extras.keySet();
                for (String key : keys) {
                    String extra = extras.getString(key);
                    startService.putExtra(key, extra);
                }
            }
            startService(startService);
        }
        Intent bindingIntent = new Intent(this, service);
        bindService(bindingIntent, serviceConnection, Context.BIND_AUTO_CREATE);
    }
    private void setFilters() {
        IntentFilter filter = new IntentFilter();
        filter.addAction(UsbService.ACTION_USB_PERMISSION_GRANTED);
        filter.addAction(UsbService.ACTION_NO_USB);
        filter.addAction(UsbService.ACTION_USB_DISCONNECTED);
        filter.addAction(UsbService.ACTION_USB_NOT_SUPPORTED);
        filter.addAction(UsbService.ACTION_USB_PERMISSION_NOT_GRANTED);
        registerReceiver(mUsbReceiver, filter);
    }
    //public final int INPUT_BUFFER_SIZE = 1024 * 16;

    /*
     * This handler will be passed to UsbService. Data received from serial port is displayed through this handler
     */
    private static class MyHandler extends Handler {
        private final WeakReference<UnityUSBActivity> mActivity;

        public MyHandler(UnityUSBActivity activity) {
            mActivity = new WeakReference<UnityUSBActivity>(activity);
        }

        @Override
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case UsbService.MESSAGE_FROM_SERIAL_PORT:
                    byte[] buffer = (byte[])msg.obj;

                    break;
                case UsbService.CTS_CHANGE:
                    Toast.makeText(mActivity.get(), "CTS_CHANGE",Toast.LENGTH_LONG).show();
                    break;
                case UsbService.DSR_CHANGE:
                    Toast.makeText(mActivity.get(), "DSR_CHANGE",Toast.LENGTH_LONG).show();
                    break;
            }
        }
    }
}