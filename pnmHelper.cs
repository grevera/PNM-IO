/**
    \file   pnmHelper.cs
    \brief  Contains pnmHelper class definition.
    \author George J. Grevera, Ph.D., ggrevera@sju.edu

    Copyright (C) 2010, George J. Grevera
 */
using System;
using System.Diagnostics;
using System.IO;

namespace CSImageViewer {

    /** \brief  This class contains methods that read and write
     *  PNM/PGM/PPM images (color rgb and grey images).
     */
    class pnmHelper {

        // input/read methods

        /** \brief  <b> This method should be generally used to read any pnm
         *  (pgm grey, ppm color) binary or ascii image files. </b>
         *  \returns an int array of pixel values;
         *  in the case of gray data, each entry is an array value;
         *  in the case of color data, the first entry is the red, the second the green, and the third the blue.
         *
         *  \param  fname  input image file name
         *  \param  w      w[0] will be set to image width
         *  \param  h      h[0] will be set to image height
         *  \param  samplesPerPixel  samplesPerPixel[0] will be set to 1 (gray) or 3 (color)
         *  \param  min    min[0] will be set to min value in image
         *  \param  max    min[0] will be set to min value in image
         *
         *  \returns  array of pixel values (rgb or gray)
         *  as well as w[0], h[0], samplesPerPixel[0], min[0], and max[0]
         *  will be set
         */
        public static int[] read_pnm_file ( String fname, int[] w, int[] h, int[] samplesPerPixel, int[] min, int[] max )
        {
            //init additional return values
            w[0] = h[0] = samplesPerPixel[0] = min[0] = max[0] = 0;
            //open the input image data file
            StreamReader sr = new StreamReader( fname );
            String  s = readHeader( sr, w, h );
            sr.Close();
            sr = null;

            //the first thing should be the file type (P2, P3, P5, or P6)
            if (s.Equals( "P2" )) {
                samplesPerPixel[ 0 ] = 1;
                return read_ascii_pgm_file( fname, w, h, min, max );
            }
            else if (s.Equals( "P3" )) {
                samplesPerPixel[ 0 ] = 3;
                return read_ascii_ppm_file( fname, w, h, min, max );
            }
            else if (s.Equals( "P5" )) {
                samplesPerPixel[ 0 ] = 1;
                return read_binary_pgm_file( fname, w, h, min, max );
            }
            else if (s.Equals( "P6" )) {
                samplesPerPixel[ 0 ] = 3;
                return read_binary_ppm_file( fname, w, h, min, max );
            }

            //create a test image (note factor of 3 below for rgb)
            /*
            samplesPerPixel[ 0 ] = 3;
            w[ 0 ] = h[ 0 ] = 256;
            int[] data = new int[ w[ 0 ] * h[ 0 ] * 3 ];
            int i = 0;
            for (int r = 0; r < 256; r++) {
                for (int c = 0; c < 256; c++) {
                    data[ i++ ] = c;  //r
                    data[ i++ ] = 0;  //g
                    data[ i++ ] = 0;  //b
                }
            }
            return data;
            */
            return null;
        }
        //----------------------------------------------------------------
        /** \brief  This function reads an ascii gray pgm file.
         *
         *  This type of file is formatted as follows:
         *  <pre>
         *    P2
         *    w h
         *    maxval
         *    v_1 v_2 v_3 . . . v_w*h
         *  </pre>
         *
         *  \param  fname  input image file name
         *  \param  w      w[0] will be set to image width
         *  \param  h      h[0] will be set to image height
         *  \param  min    min[0] will be set to min value in image
         *  \param  max    min[0] will be set to min value in image
         *
         *  \returns  array of pixel values (gray)
         *  as well as w[0], h[0], min[0], and max[0] will be set
         */
        protected static int[] read_ascii_pgm_file ( String fname, int[] w, int[] h, int[] min, int[] max )
        {
            StreamReader  sr = new StreamReader( fname );
            String  s = readHeader( sr, w, h );
            //the file type should be P2
            Debug.Assert( s.Equals( "P2" ) );
            if (!s.Equals( "P2" )) {
                sr.Close();
                sr = null;
                return null;
            }
            int[]  data = read_ascii_ints( sr, w[0]*h[0], max, min );
            sr.Close();
            sr = null;
            return data;
        }
        //----------------------------------------------------------------
        /** \brief This function reads an ascii color ppm file.
         *
         *  This type of file is formatted as follows:
         *  <pre>
         *    P3
         *    w h
         *    maxval
         *    vr_1 vg_1 vb_1
         *    vr_2 vg_2 vb_2
         *    . . .
         *    vr_w*h vg_w*h vb_w*h
         *  </pre>
         *
         *  \param  fname  input image file name
         *  \param  w      w[0] will be set to image width
         *  \param  h      h[0] will be set to image height
         *  \param  min    min[0] will be set to min value in image
         *  \param  max    min[0] will be set to min value in image
         *
         *  \returns  array of pixel values (color)
         *  as well as w[0], h[0], min[0], and max[0] will be set
         */
        protected static int[] read_ascii_ppm_file ( String fname, int[] w, int[] h, int[] min, int[] max )
        {
            StreamReader  sr = new StreamReader( fname );
            String  s = readHeader( sr, w, h );
            //the file type should be P3
            Debug.Assert( s.Equals( "P3" ) );
            if (!s.Equals( "P3" )) {
                sr.Close();
                sr = null;
                return null;
            }
            int[]  data = read_ascii_ints( sr, w[0]*h[0]*3, max, min );
            sr.Close();
            sr = null;
            return data;
        }
        //----------------------------------------------------------------
        /** \brief This function reads a binary gray pgm file.
         *
         *  This type of file is formatted as follows:
         *  <pre>
         *    P5
         *    w h
         *    maxval
         *    v_1 v_2 v_3 . . . v_w*h
         *  </pre>
         *  One 8-bit byte per binary value.
         * 
         *  \param  fname  input image file name
         *  \param  w      w[0] will be set to image width
         *  \param  h      h[0] will be set to image height
         *  \param  min    min[0] will be set to min value in image
         *  \param  max    min[0] will be set to min value in image
         *
         *  \returns  array of pixel values (gray)
         *  as well as w[0], h[0], min[0], and max[0] will be set
         */
        protected static int[] read_binary_pgm_file ( String fname, int[] w, int[] h, int[] min, int[] max )
        {
            StreamReader  sr = new StreamReader( fname );
            String  s = readHeader( sr, w, h );
            //the file type should be P5
            Debug.Assert( s.Equals( "P5" ) );
            if (!s.Equals( "P5" )) {
                sr.Close();
                sr = null;
                return null;
            }
            sr.Close();
            sr = null;
            int[]  data = read_binary_int8s( fname, w[0]*h[0], max, min );
            return data;
        }
        //----------------------------------------------------------------
        /** \brief This function reads a binary color (rgb, 8-bits per
         *  component/24-bits per pixel) ppm file.
         *
         *  This type of file is formatted as follows:
         *  <pre>
         *    P6
         *    w h
         *    maxval
         *    vr_1 vg_1 vb_1 
         *    vr_2 vg_2 vb_2
         *    . . .
         *    vr_w*h vg_w*h vb_w*h
         *  </pre>
         *  One 8-bit byte per binary value (3 bytes for each RGB value).
         *
         *  \param  fname  input image file name
         *  \param  w      w[0] will be set to image width
         *  \param  h      h[0] will be set to image height
         *  \param  min    min[0] will be set to min value in image
         *  \param  max    min[0] will be set to min value in image
         *
         *  \returns  array of pixel values (color)
         *  as well as w[0], h[0], min[0], and max[0] will be set
         */
        protected static int[] read_binary_ppm_file ( String fname, int[] w, int[] h, int[] min, int[] max )
        {
            StreamReader  sr = new StreamReader( fname );
            String  s = readHeader( sr, w, h );
            //the file type should be P6
            Debug.Assert( s.Equals( "P6" ) );
            if (!s.Equals( "P6" )) {
                sr.Close();
                sr = null;
                return null;
            }
            sr.Close();
            sr = null;
            int[]  data = read_binary_int8s( fname, w[0]*h[0]*3, max, min );
            return data;
        }
        //----------------------------------------------------------------
        /** \brief  <b>Non-standard</b> function that reads 16-bit values
         *  as a binary pgm file (<b>not implemented yet</b>).
         *
         *  \param  fname  input image file name
         *  \param  w      w[0] will be set to image width
         *  \param  h      h[0] will be set to image height
         *  \param  min    min[0] will be set to min value in image
         *  \param  max    min[0] will be set to min value in image
         *
         *  \returns  array of pixel values (gray)
         *  as well as w[0], h[0], min[0], and max[0] will be set
         */
        public static int[] read_binary_pgm16_file ( String fname, int[] w, int[] h, int[] min, int[] max )
        {
            StreamReader  sr = new StreamReader( fname );
            readHeader( sr, w, h );
            sr.Close();
            sr = null;
            int[]  data = read_binary_int16s( fname, w[0]*h[0], max, min );
            return data;
        }
        //----------------------------------------------------------------
        /** \brief  <b>Non-standard</b> function that reads 32-bit values
         *  as a binary pgm file (<b>not implemented yet</b>).
         *
         *  \param  fname  input image file name
         *  \param  w      w[0] will be set to image width
         *  \param  h      h[0] will be set to image height
         *  \param  min    min[0] will be set to min value in image
         *  \param  max    min[0] will be set to min value in image
         *
         *  \returns  array of pixel values (gray)
         *  as well as w[0], h[0], min[0], and max[0] will be set
         */
        public static int[] read_binary_pgm32_file ( String fname, int[] w, int[] h, int[] min, int[] max )
        {
            StreamReader  sr = new StreamReader( fname );
            readHeader( sr, w, h );
            sr.Close();
            sr = null;
            int[]  data = read_binary_int32s( fname, w[ 0 ] * h[ 0 ], max, min );
            return data;
        }
        //----------------------------------------------------------------
        /** \brief    Read image file header.
         * 
         *  \param    sr  input image stream
         *  \param    w   w[0] will be set to image width
         *  \param    h   h[0] will be set to image height
         * 
         *  \returns  string indicating image file type
         *  as well as w[0] and h[0] will be set
         */
        protected static String readHeader ( StreamReader sr, int[] w, int[] h )
        {
            w[0] = h[0] = 0;

            //skip comments (if any)
            String  s = "#";
            while (s.StartsWith( "#" )) {
                s = sr.ReadLine();
            }

            //the first thing should be the file type (P2, P3, P5, or P6)
            Debug.Assert( s.Equals("P2") || s.Equals("P3") || s.Equals("P5") || s.Equals("P6") );
            if (!s.Equals("P2") && !s.Equals("P3") && !s.Equals("P5") && !s.Equals("P6")) {
                sr.Close();
                sr = null;
                return null;
            }
            String fileType = s;

            //skip comments (if any)
            s = "#";
            while (s.StartsWith( "#" )) {
                s = sr.ReadLine();
            }

            String[] wh = s.Split();
            w[ 0 ] = Int32.Parse( wh[ 0 ] );
            h[ 0 ] = Int32.Parse( wh[ 1 ] );
            int[] data = new int[ w[ 0 ] * h[ 0 ] * 3 ];

            //skip comments (if any)
            s = "#";
            while (s.StartsWith( "#" )) {
                s = sr.ReadLine();
            }
            //what's currently in s from the last read above is the max value which we won't use

            return fileType;
        }
        //----------------------------------------------------------------
        /** \brief  This function reads binary 8-bit integer values from a
         *  file.
         * 
         *  \param  fname    input image file name
         *  \param  howMany  the number of values to read
         *  \param  min      min[0] will be set to min value in image
         *  \param  max      min[0] will be set to min value in image
         *
         *  \returns  array of pixel values
         *  as well as min[0] and max[0] will be set
         */
        protected static int[] read_binary_int8s ( String fname, int howMany, int[] max, int[] min )
        {
            int[]  data = new int[ howMany ];
            min[0] = Int32.MaxValue;
            max[0] = Int32.MinValue;
            BinaryReader br = new BinaryReader( File.Open( fname, FileMode.Open ) );
            //calculate the size of the header so that we can skip it
            long  len  = br.BaseStream.Length;
            long  diff = len - howMany;
            Debug.Assert( diff >= 0 );
            //skip the header
            for (int i=0; i<diff; i++) {
                br.ReadByte();
            }
            //read the binary data
            for (int i=0; i<howMany; i++) {
                int  value = br.ReadByte();
                if (value < min[0])    min[0] = value;
                if (value > max[0])    max[0] = value;
                data[i] = value;
            }
            return data;
        }
        //----------------------------------------------------------------
        /** \brief  This function reads binary 16-bit integer values from a
         *  file.
         * 
         *  \param  fname    input image file name
         *  \param  howMany  the number of values to read
         *  \param  min      min[0] will be set to min value in image
         *  \param  max      min[0] will be set to min value in image
         *
         *  \returns  array of pixel values
         *  as well as min[0] and max[0] will be set
         */
        protected static int[] read_binary_int16s ( String fname, int howMany, int[] max, int[] min )
        {
            int[]  data = new int[ howMany ];
            min[ 0 ] = Int32.MaxValue;
            max[ 0 ] = Int32.MinValue;
            BinaryReader  br = new BinaryReader( File.Open( fname, FileMode.Open ) );
            //calculate the size of the header so that we can skip it
            long  len  = br.BaseStream.Length;
            long  diff = len - howMany;
            Debug.Assert( diff >= 0 );
            //skip the header
            for (int i = 0; i < diff; i++) {
                br.ReadByte();
            }
            //read the binary data
            for (int i = 0; i < howMany; i++) {
                int value = br.ReadInt16();
                if (value < min[ 0 ])    min[ 0 ] = value;
                if (value > max[ 0 ])    max[ 0 ] = value;
                data[ i ] = value;
            }
            return data;
        }
        //----------------------------------------------------------------
        /** \brief  This function reads binary 32-bit integer values from a
         *  file.
         * 
         *  \param  fname    input image file name
         *  \param  howMany  the number of values to read
         *  \param  min      min[0] will be set to min value in image
         *  \param  max      min[0] will be set to min value in image
         *
         *  \returns  array of pixel values
         *  as well as min[0] and max[0] will be set
         */
        protected static int[] read_binary_int32s ( String fname, int howMany, int[] max, int[] min ) {
            int[]  data = new int[ howMany ];
            min[ 0 ] = Int32.MaxValue;
            max[ 0 ] = Int32.MinValue;
            BinaryReader  br = new BinaryReader( File.Open( fname, FileMode.Open ) );
            //calculate the size of the header so that we can skip it
            long  len  = br.BaseStream.Length;
            long  diff = len - howMany;
            Debug.Assert( diff >= 0 );
            //skip the header
            for (int i = 0; i < diff; i++) {
                br.ReadByte();
            }
            //read the binary data
            for (int i = 0; i < howMany; i++) {
                int  value = br.ReadInt32();
                if (value < min[ 0 ])    min[ 0 ] = value;
                if (value > max[ 0 ])    max[ 0 ] = value;
                data[ i ] = value;
            }
            return data;
        }
        //----------------------------------------------------------------
        /** \brief  This function reads ascii integer (8-bits or more)
         *  values from a file.
         * 
         *  \param  sr       input image file stream
         *  \param  howMany  the number of values to read
         *  \param  min      min[0] will be set to min value in image
         *  \param  max      min[0] will be set to min value in image
         *
         *  \returns  array of pixel values
         *  as well as min[0] and max[0] will be set
         */
        protected static int[] read_ascii_ints ( StreamReader sr, int howMany, int[] max, int[] min )
        {
            int[]  data = new int[ howMany ];
            min[ 0 ] = Int32.MaxValue;
            max[ 0 ] = Int32.MinValue;
            for (int i = 0; i < howMany; i++) {
                int  tmp;
                char  ch;
                String  digits = "";
                //skip leading whitespace (non-digits) (if any)
                for (; ; ) {
                    if (sr.EndOfStream) break;
                    tmp = sr.Read();
                    ch = Convert.ToChar( tmp );
                    if ('0' <= ch && ch <= '9') {
                        digits += ch;
                        break;
                    }
                }
                Debug.Assert( digits.Length > 0 );
                //keep reading digits until we find a non-digit (or eof)
                for (; ; ) {
                    if (sr.EndOfStream) break;
                    tmp = sr.Read();
                    ch = Convert.ToChar( tmp );
                    if (ch < '0' || ch > '9') break;
                    digits += ch;
                }
                Debug.Assert( digits.Length > 0 );
                int value = Int32.Parse( digits );
                if (value < min[0])    min[0] = value;
                if (value > max[0])    max[0] = value;
                data[i] = value;
            }
            return data;
        }

        //================================================================
        // output/write methods.
        // Note:  gimp seems to really care about the \r's and \n's!
        //================================================================

        /** \brief    Standard function that writes values as a pgm (grey)
         *  or ppm (color) ascii file.
         * 
         *  \param    fname              input image file name
         *  \param    buff               buffer of pixel values to write to the file
         *  \param    width              image width
         *  \param    height             image height
         *  \param    samples_per_pixel  samples per pixel (1=gray; 3=rgb/color)
         * 
         *  \returns  nothing (void)
         */
        public static void write_pgm_or_ppm_ascii_data ( String fname, int[] buff, int width, int height, int samples_per_pixel )
        {
            Debug.Assert( buff != null, "buff must not be null" );
            StreamWriter  wr = File.CreateText( fname );
            if (samples_per_pixel==1)         wr.Write( "P2\n" );
            else if (samples_per_pixel==3)    wr.Write( "P3\n" );
            else                              Debug.Assert( false );
            wr.Write( "# created by george (ASCII, obviously)\n" );
            wr.Write( width );
            wr.Write( " " );
            wr.Write( height );
            wr.Write( "\n" );

            //determine the max value
            int  maxval = buff[ 0 ];
            for (int i = 0; i < width * height * samples_per_pixel; i++) {
                if (buff[ i ] > maxval)    maxval = buff[ i ];
            }
            if (maxval == 0)    maxval = 255;
            //write the max value
            wr.Write( maxval );
            wr.Write( "\n" );

            //write the individual pixel values
            for (int i = 0; i < width * height * samples_per_pixel; i++) {
                wr.Write( " " );
                wr.Write( buff[ i ] );
                if (i>0 && i%10==0)    wr.Write( "\n" );
            }

            wr.Close();
        }
        //----------------------------------------------------------------
        /** \brief    Standard functions that writes 8-bit values as a binary
         *  pgm (gray) or ppm (color) file.
         * 
         *  \param    fname              input image file name
         *  \param    buff               buffer of pixel values to write to the file
         *  \param    width              image width
         *  \param    height             image height
         *  \param    samples_per_pixel  samples per pixel (1=gray; 3=rgb/color)
         * 
         *  \returns  nothing (void)
         */
        public static void write_binary_pgm_or_ppm_data8 ( String fname, int[] buff, int width, int height, int samples_per_pixel )
        {
            Debug.Assert( buff != null );
            BinaryWriter  wr = new BinaryWriter( File.Open( fname, FileMode.Create ) );
            String  s = "";
            if      (samples_per_pixel == 1)    s += "P5\r\n";
            else if (samples_per_pixel == 3)    s += "P6\r\n";
            else                                Debug.Assert( false );
            s += "# created by george (raw-8, not-so-obviously\r\n";
            s += width + " " + height + "\r\n";
            //determine the max value
            int  maxval = buff[ 0 ];
            for (int i = 0; i < width * height * samples_per_pixel; i++) {
                if (buff[ i ] > maxval)    maxval = buff[ i ];
            }
            if (maxval == 0)    maxval = 255;
            //write the max value and other header info
            s += maxval + "\n";
            writeString( wr, s );

            /** \todo below could be made more efficient by replacing it with fewer, larger buffer writes */
            //write the individual pixel values
            for (int i = 0; i < width * height * samples_per_pixel; i++) {
                byte  v = (byte) buff[ i ];
                wr.Write( v );
            }

            wr.Close();
        }
        //----------------------------------------------------------------
        /** \brief    <b>Non-standard</b> function that writes 16-bit values
         *  as a binary pgm (gray only) file.
         * 
         *  \param    fname              input image file name
         *  \param    buff               buffer of pixel values to write to the file
         *  \param    width              image width
         *  \param    height             image height
         * 
         *  \returns  nothing (void)
         */
        public static void write_binary_pgm_data16 ( String fname, int[] buff, int width, int height ) {
            Debug.Assert( buff != null );
            BinaryWriter wr = new BinaryWriter( File.Open( fname, FileMode.Create ) );
            String  s = "P5\r\n"
                      + "# created by george (raw-16, not-so-obviously)\r\n"
                      + width + " " + height + "\r\n";
            //determine the max value
            int  maxval = buff[ 0 ];
            for (int i = 0; i < width * height; i++) {
                if (buff[ i ] > maxval)    maxval = buff[ i ];
            }
            if (maxval == 0)    maxval = 255;
            //write the max value and other header info
            s += maxval + "\n";
            writeString( wr, s );

            /** \todo below could be made more efficient by replacing it with fewer, larger buffer writes */
            //write the individual pixel values
            for (int i = 0; i < width * height; i++) {
                short  v = (short) buff[ i ];
                wr.Write( v );
            }

            wr.Close();
        }
        //----------------------------------------------------------------
        /** \brief    <b>Non-standard</b> function that writes 32-bit values
         *  as a binary pgm (gray only) file.
         * 
         *  \param    fname              input image file name
         *  \param    buff               buffer of pixel values to write to the file
         *  \param    width              image width
         *  \param    height             image height
         * 
         *  \returns  nothing (void)
         */
        public static void write_binary_pgm_data32 ( String fname, int[] buff, int width, int height ) {
            Debug.Assert( buff != null );
            BinaryWriter wr = new BinaryWriter( File.Open( fname, FileMode.Create ) );
            String  s = "P5\r\n"
                      + "# created by george (raw-16, not-so-obviously)\r\n"
                      + width + " " + height + "\r\n";
            //determine the max value
            int  maxval = buff[ 0 ];
            for (int i = 0; i < width * height; i++) {
                if (buff[ i ] > maxval)    maxval = buff[ i ];
            }
            if (maxval == 0)    maxval = 255;
            //write the max value and other header info
            s += maxval + "\n";
            writeString( wr, s );

            /** \todo below could be made more efficient by replacing it with fewer, larger buffer writes */
            //write the individual pixel values
            for (int i = 0; i < width * height; i++) {
                int  v = buff[ i ];
                wr.Write( v );
            }

            wr.Close();
        }
        //----------------------------------------------------------------
        /** \brief    When I do binary writes of strings in C#, it appears
         *  that they are stored as ascii counted strings (so the count
         *  appears before the string.  This function simply writes out the
         *  characters in the string.
         * 
         *  \param    wr  output binary stream
         *  \param    s   string to write
         * 
         *  \returns  nothing (void)
         */
        private static void writeString ( BinaryWriter wr, String s ) {
            for (int i = 0; i < s.Length; i++) {
                wr.Write( s[ i ] );
            }
        }

    }  //end class

}  //end namespace
