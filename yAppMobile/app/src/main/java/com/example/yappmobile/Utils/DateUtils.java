package com.example.yappmobile.Utils;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.time.ZoneId;


public class DateUtils {
    public static String convertUtcToFormattedTime(String utcTime) {
        // Parse the ISO-8601 timestamp
        ZonedDateTime zonedDateTime = ZonedDateTime.parse(utcTime);

        // Convert the ZonedDateTime to the local timezone
        ZonedDateTime localDateTime = zonedDateTime.withZoneSameInstant(ZoneId.systemDefault());

        // Define the desired format
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("MM/dd/yyyy, hh:mm:ss a");

        // Return the formatted date-time string
        return localDateTime.format(formatter);
    }
}
