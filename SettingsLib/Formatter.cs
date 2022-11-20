using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace SettingsLib
{
  internal class Formatter
  {
#pragma warning disable CS0168 // Variable is declared but never used

    /// <summary>
    /// Reads from the open stream one entry
    /// </summary>
    /// <param name="jStream">An open stream at position</param>
    /// <returns>A Controller obj or null for errors</returns>
    public static T FromJsonStream<T>( Stream jStream )
    {
      try {
        var jsonSerializer = new DataContractJsonSerializer( typeof( T ) );
        object objResponse = jsonSerializer.ReadObject( jStream );
        var jsonResults = (T)objResponse;
        jStream.Flush( );
        return jsonResults;
      }
      catch (Exception e) {
        return default( T );
      }
    }


    /// <summary>
    /// Reads from a file one entry
    /// Tries to aquire a shared Read Access and blocks for max 100ms while doing so
    /// </summary>
    /// <param name="jFilename">The Json Filename</param>
    /// <returns>A Controller obj or null for errors</returns>
    public static T FromJsonFile<T>( string jFilename )
    {
      T retVal = default( T );
      if (!File.Exists( jFilename )) {
        return retVal;
      }

      int retries = 10; // 100ms worst case
      while (retries-- > 0) {
        try {
          using (var ts = File.Open( jFilename, FileMode.Open, FileAccess.Read, FileShare.Read )) {
            retVal = FromJsonStream<T>( ts );
          }
        }
        catch (IOException ioex) {
          // retry after a short wait
          Thread.Sleep( 10 ); // allow the others fileIO to be completed
        }
        catch (Exception ex) {
          // not an IO exception - just fail
          return retVal;
        }
      }

      return retVal;
    }


    /// <summary>
    /// Write to the open stream one entry
    /// </summary>
    /// <param name="data">A datafile object to write</param>
    /// <param name="jStream">An open stream at position</param>
    /// <returns>True if successfull</returns>
    public static bool ToJsonStream<T>( T data, Stream jStream )
    {
      try {
        var jsonSerializer = new DataContractJsonSerializer( typeof( T ) );
        jsonSerializer.WriteObject( jStream, data );
        return true;
      }
      catch (Exception e) {
        return false; // bails at data==null or formatting issues
      }
    }

    /// <summary>
    /// Write to a file one entry
    /// Tries to aquire an exclusive Write Access and blocks for max 100ms while doing so
    /// </summary>
    /// <param name="data">A datafile object to write</param>
    /// <param name="jFilename">The Json Filename</param>
    /// <returns>True if successfull</returns>
    public static bool ToJsonFile<T>( T data, string jFilename )
    {
      bool retVal = false;

      int retries = 10; // 100ms worst case
      while (retries-- > 0) {
        try {
          using (var ts = File.Open( jFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None )) {
            retVal = ToJsonStream( data, ts );
          }
        }
        catch (IOException ioex) {
          // retry after a short wait
          Thread.Sleep( 10 ); // allow the others fileIO to be completed
        }
        catch (Exception ex) {
          // not an IO exception - just fail
          return retVal;
        }
      }
      return retVal;
    }


    /// <summary>
    /// Locked Update of a Json File
    /// It uses the Update Function to send the most current data to the caller and expects updated data in return
    /// Tries to aquire an exclusive Write Access and blocks for max 100ms while doing so
    /// 
    /// </summary>
    /// <param name="jFilename">The Json Filename</param>
    /// <param name="updateFunction">A function to update the argument an return the new data to save</param>
    /// <returns></returns>
    public static bool UpdateJsonFile<T>( string jFilename, Func<T,T> updateFunction )
    {
      bool retVal = false;

      int retries = 10; // 100ms worst case
      while (retries-- > 0) {
        try {
          // read/write within the lock
          using (var ts = File.Open( jFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None )) {
            T data= FromJsonStream<T>( ts );
            // call Update function
            data = updateFunction( data );
            // rewind, clear and write back to the file stream
            ts.Seek( 0, SeekOrigin.Begin );
            ts.SetLength( 0 );
            retVal = ToJsonStream( data, ts );
          }
          return retVal; //20221109-Fix - return if no Exception was raised
        }
        catch (IOException ioex) {
          // retry after a short wait
          Thread.Sleep( 10 ); // allow the others fileIO to be completed
        }
        catch (Exception ex) {
          // not an IO exception - just fail
          return retVal;
        }
      }

      return retVal;
    }

#pragma warning restore CS0168 // Variable is declared but never used
  }
}
