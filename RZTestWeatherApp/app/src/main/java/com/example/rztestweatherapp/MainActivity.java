package com.example.rztestweatherapp;

import androidx.appcompat.app.AppCompatActivity;

import android.os.AsyncTask;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

public class MainActivity extends AppCompatActivity {

    private EditText editText_town;
    private TextView text_header;
    private TextView text_result;
    private Button button_main;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        editText_town = findViewById(R.id.editText_town);
        text_result = findViewById(R.id.text_result);
        button_main = findViewById(R.id.button_main);

        button_main.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (editText_town.getText().toString().trim().equals(""))
                    Toast.makeText(MainActivity.this, R.string.str_no_text, Toast.LENGTH_LONG).show();
                else
                {
                    String city = editText_town.getText().toString();
                    String api = "68baa3ba87e38acd3d9ed0a404753b1e";
                    String url = "https://api.openweathermap.org/data/2.5/weather?q="+city+ "&appid="+api+"&units=metric&&lang=ru";

                    (new GetURLData()).execute(url);

                }

            }
        });
    }


    private class GetURLData extends AsyncTask<String, String, String>
    {
        @Override
        protected void onPreExecute() {
            super.onPreExecute();
            text_result.setText("Ожидайте...");

        }

        @Override
        protected String doInBackground(String... strings) {

            HttpURLConnection connection = null;
            BufferedReader reader = null;

            try {
                URL url = new URL(strings[0]);
                connection = (HttpURLConnection) url.openConnection();
                connection.connect();

                InputStream stream = connection.getInputStream();
                reader = new BufferedReader(new InputStreamReader(stream));

                StringBuffer buffer = new StringBuffer();
                String line = "";

                while ((line = reader.readLine()) != null)
                {
                    buffer.append(line).append("\n");

                }
                return buffer.toString();

            } catch (MalformedURLException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            } finally {
                if (connection != null)
                connection.disconnect();


                    try {
                        if (reader != null)
                            reader.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }

            }
            return null;
        }

        @Override
        protected void onPostExecute(String result) {
            super.onPostExecute(result);

            try {
                JSONObject json = new JSONObject(result);
                result = "Температура: " + json.getJSONObject("main").getDouble("temp");
            } catch (JSONException e) {
                e.printStackTrace();
            }

            text_result.setText(result);

        }

    };



}

