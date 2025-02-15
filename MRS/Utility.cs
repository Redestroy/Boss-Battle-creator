using System;
public class Utility{
    public static long GetNowUnixTime(){
	    DateTime currentTime = DateTime.UtcNow;
	    long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
        return unixTime;
    }

    public static string ConvertUnixTimeToDate(long unixTime){
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds( unixTime ).ToLocalTime();
        return dateTime.ToString();
    }
}