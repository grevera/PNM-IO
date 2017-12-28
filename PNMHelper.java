/**
 *  file:  PNMHelper.java
 *  Header file for PNM image readers & writers.
 *
 *  Update regarding 16-bit support:
 *    "Each gray value is a number from 0 through Maxval, with 0 being
 *    black and Maxval being white.  Each gray value is represented in 
 *    pure binary by either 1 or 2 bytes.  If the Maxval is less than 
 *    256, it is 1 byte.  Otherwise, it is 2 bytes.  The most significant 
 *    byte is first."
 *      - from http://netpbm.sourceforge.net/doc/pgm.html and
 *        http://netpbm.sourceforge.net/doc/ppm.html
 *    Note:  Intel is little endian.
 *
 *  @author George J. Grevera, Ph.D., ggrevera@sju.edu
 *
 *  Copyright (C) 2013, George J. Grevera
 */
import  java.io.FileInputStream;
import  java.io.FileOutputStream;
import  java.io.PrintWriter;
import  java.io.RandomAccessFile;
import  java.util.Scanner;
//----------------------------------------------------------------------
/** This class contains methods that read and write PNM images (color rgb
 *  and grey images).
 */
public class PNMHelper {
/*
    public static void main ( String[] args ) {
        java.io.File    file  = new java.io.File( ".." );
        String          path  = file.getAbsolutePath();
        //read directory contents
        java.io.File[]  files = file.listFiles();
        //result = (System.setProperty("user.dir", directory.getAbsolutePath()) != null);
        
        PNMHelper  p = new PNMHelper( "c:\\52D32U7JPG.ppm");
        System.out.println( p.mW );
        System.out.println( p.mH );
        System.out.println( p.mSamplesPerPixel );
        System.out.println( p.mMin );
        System.out.println( p.mMax );
        
        p.saveAscii( "C:\\fred.ppm" );
        p.saveBinary( "C:\\fred-binary.ppm" );
        System.out.println( "done" );
    }
*/
    /** input image file name */
    private String  mFileName;
    /** image width */
    private int     mW;
    /** image height */
    private int     mH;
    /** image samples per pixel (1=gray, 3=color) */
    private int     mSamplesPerPixel;
    /** minimum of image data */
    public int      mMin;
    /** maximum of image data */
    public int      mMax;
    /** image pixel data */
    public int[]    mData;
    //--------------------------------------------------------------------
    /**
     * ctor for an empty image of the specified width, height, and type.
     * @param width   is the image width
     * @param height  is the image height
     * @param samples 1=gray, 3=color
     */
    public PNMHelper ( int width, int height, int samples ) {
        assert samples==1 || samples==3;
        mW = width;
        mH = height;
        mSamplesPerPixel = samples;
        mData = new int[ mW * mH * mSamplesPerPixel ];
    }
    //....................................................................
    /**
     * ctor for an empty image of the specified width, height, and type=gray.
     * @param width   is the image width
     * @param height  is the image height
     */
    public PNMHelper ( int width, int height ) {  this( width, height, 1 );  }
    //....................................................................
    /**
     * ctor that loads an image from a file
     * @param fname is the file name
     */
    public PNMHelper ( String fname ) {
        mFileName = fname;
        Scanner  in = null;
        try {
            in = new Scanner( new FileInputStream(fname) );
        } catch (Exception e) {
            System.err.println( "PNMHelper:PNMHelper: " + e );
            return;
        }
        
        //skip comment lines (if any)
        String  ln = null;
        while (in.hasNextLine()) {
            ln = in.nextLine();
            if (!ln.startsWith("#"))    break;
        }
        assert ln != null;           //could be EOF
        assert !ln.startsWith("#");  //could be comments
        
        //save file type for later
        String  fileType = ln;
        
        //skip comment lines (if any)
        ln = null;
        while (in.hasNextLine()) {
            ln = in.nextLine();
            if (!ln.startsWith("#"))    break;
        }
        assert ln != null;           //could be EOF
        assert !ln.startsWith("#");  //could be comments
        
        //should be w h
        String[]  parts = ln.split("\\s+");
        mW = Integer.parseInt( parts[0] );
        mH = Integer.parseInt( parts[1] );
        
        //skip comment lines (if any)
        ln = null;
        while (in.hasNextLine()) {
            ln = in.nextLine();
            if (!ln.startsWith("#"))    break;
        }
        assert ln != null;           //could be EOF
        assert !ln.startsWith("#");  //could be comments
        //what remains is/should be max
        mMax = Integer.parseInt( ln );
        
        //determine image file type
        //the first thing should be the file type (P2, P3, P5, or P6)
        if        (fileType.equals( "P2" )) {
            mSamplesPerPixel = 1;
            read_ascii_data( in );
        } else if (fileType.equals( "P3" )) {
            mSamplesPerPixel = 3;
            read_ascii_data( in );
        } else if (fileType.equals( "P5" )) {
            mSamplesPerPixel = 1;
            read_binary_data();
        } else if (fileType.equals( "P6" )) {
            mSamplesPerPixel = 3;
            read_binary_data();
        } else {
            assert false;  //should never get here
        }
        
        in.close();
    }
    //--------------------------------------------------------------------
    /**
     * read in ASCII image data
     * @param in is the input stream that is already positioned at the data
     */
    private void read_ascii_data ( Scanner in ) {
        mData = new int[ mW*mH*mSamplesPerPixel ];
        int  tMax = 0;
        for (int i=0; i<mData.length; i++) {
            mData[i] = in.nextInt();
            if (i==0)               mMin = tMax = mData[0];
            if (mData[i] < mMin)    mMin = mData[i];
            if (mData[i] > tMax)    tMax = mData[i];
        }
        assert tMax == mMax;
    }
    //....................................................................
    /**
     * read binary image data from a file (specified by mFileName)
     */
    private void read_binary_data ( ) {
        mData = new int[ mW*mH*mSamplesPerPixel ];
        int  tMax = 0;
        try {
            RandomAccessFile  in = new RandomAccessFile( mFileName, "r" );
            if (mMax<256) {
                in.seek( in.length() - mData.length );
                for (int i=0; i<mData.length; i++) {
                    mData[i] = in.read();
                    if (i==0)               mMin = tMax = mData[0];
                    if (mData[i] < mMin)    mMin = mData[i];
                    if (mData[i] > tMax)    tMax = mData[i];
                }
            } else {
                assert mMax <= Short.MAX_VALUE;
                in.seek( in.length() - mData.length*2 );
                for (int i=0; i<mData.length; i++) {
                    int  hi = in.read();
                    int  lo = in.read();
                    mData[i] = (hi << 8) | lo;
                    if (i==0)               mMin = tMax = mData[0];
                    if (mData[i] < mMin)    mMin = mData[i];
                    if (mData[i] > tMax)    tMax = mData[i];
                }
            }
            in.close();
        } catch (Exception e) {
            System.err.println( "PNMHelper:read_binary_data: " + e );
        }
        assert tMax == mMax;
    }
    //--------------------------------------------------------------------
    /**
     * save data to an ASCII ppm or pgm file using the image data
     * @param fname is the output file name
     */
    public void save ( String fname ) {  saveAscii( fname );  }
    //....................................................................
    /**
     * save data to an ASCII ppm or pgm file using the image data
     * @param fname is the output file name
     */
    public void saveAscii ( String fname ) {
        PrintWriter  out = null;
        try {
            out = new PrintWriter( new FileOutputStream(fname) );
        } catch (Exception e) {
            System.err.println( "PNMHelper:saveAscii: " + e );
            return;
        }
        
        //write the header
        if (mSamplesPerPixel == 1) {
            out.println( "P2" );
            out.println( "# created by george (gray ASCII, obviously)" );
        } else if (mSamplesPerPixel == 3) {
            out.println( "P3" );
            out.println( "# created by george (color ASCII, obviously)" );
        } else {
            out.close();
            assert false;
            return;
        }
        out.println( mW + " " + mH );
        setMinMax();
        out.println( mMax );
        
        //write the data
        for (int i=0; i<mData.length; i++)    out.print( mData[i] + " " );
        out.println();
        
        out.close();
    }
    //....................................................................
    /**
     * save image data to a binary ppm or pgm file
     * @param fname is the output file name
     */
    public void saveBinary ( String fname ) {
        try {
            RandomAccessFile  out = new RandomAccessFile( fname, "rw" );
            //write the header
            if (mSamplesPerPixel == 1) {
                out.writeBytes( "P5\n" );
                out.writeBytes( "# created by george (gray binary, obviously)\n" );
            } else if (mSamplesPerPixel == 3) {
                out.writeBytes( "P6\n" );
                out.writeBytes( "# created by george (color binary, obviously)\n" );
            } else {
                out.close();
                assert false;
                return;
            }
            out.writeBytes( mW + " " + mH + "\n" );
            setMinMax();
            if (mMin < 0)
                System.err.println( "PNMHelper:saveBinary: min (" + mMin + ") is less than zero." );
            out.writeBytes( mMax + "\n" );
            //write the data
            if (mMax < 256) {
                for (int i=0; i<mData.length; i++)    out.write( mData[i] );
            } else {
                assert mMax <= Short.MAX_VALUE;
                for (int i=0; i<mData.length; i++) {
                    int  hi = mData[i] >> 8;
                    int  lo = mData[i] & 0xff;
                    out.write( hi );
                    out.write( lo );
                }
            }
            out.close();
        } catch (Exception e) {
            System.err.println( "PNMHelper:saveBinary: " + e );
        }
    }
    //--------------------------------------------------------------------
    /**
     * set the min and max fields to the min and max values of the data
     */
    private void setMinMax ( ) {
        mMin = mMax = mData[0];
        for (int i=0; i<mData.length; i++) {
            if (mData[i] < mMin)    mMin = mData[i];
            if (mData[i] > mMax)    mMax = mData[i];
        }
        
        if (mMin < 0)
            System.err.println( "PNMHelper:setMinMax: min<0!" );
        if (mMax > 255)
            System.err.println( "PNMHelper:setMinMax: max>255 may cause problems with binary image files and some readers (but not this one)!" );
        if (mMax == mMin) {
            mMax = 255;
            System.err.println( "PNMHelper:setMinMax: max==min!  Setting max to 255." );
        }
    }
    
}
