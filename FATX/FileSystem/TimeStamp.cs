﻿using System;

namespace FATX.FileSystem
{
    public class X360TimeStamp : TimeStamp
    {
        public X360TimeStamp(uint time)
            : base(time)
        {

        }
        public override int Year => base.Year + 1980;
    }

    public class XTimeStamp : TimeStamp
    {
        public XTimeStamp(uint time)
            : base(time)
        {

        }
        public override int Year => base.Year + 2000;
    }

    public class TimeStamp
    {
        private uint _Time;
        private DateTime? _DateTime;
        private readonly DateTime _minWinFileTime = new DateTime(1601, 01, 01);

        public TimeStamp(uint time)
        {
            this._Time = time;
        }
        public virtual int Year
        {
            get { return (int)((this._Time & 0xFE000000) >> 25); }
        }

        public virtual int Month
        {
            get { return (int)((this._Time & 0x1E00000) >> 21); }
        }

        public virtual int Day
        {
            get { return (int)((this._Time & 0x1F0000) >> 16); }
        }

        public virtual int Hour
        {
            get { return (int)((this._Time & 0xF800) >> 11); }
        }

        public virtual int Minute
        {
            get { return (int)((this._Time & 0x7E0) >> 5); }
        }

        public virtual int Second
        {
            get { return (int)((this._Time & 0x1F) * 2); }
        }

        public uint AsInteger()
        {
            return _Time;
        }

        public DateTime AsDateTime()
        {
            if (this._DateTime.HasValue)
            {
                if (this._DateTime < _minWinFileTime)
                {
                    return _minWinFileTime;
                }

                return this._DateTime.Value;
            }
            else
            {
                try
                {
                    _DateTime = new DateTime(
                        this.Year, this.Month,
                        this.Day, this.Hour,
                        this.Minute, this.Second);
                    return _DateTime.Value;
                }
                catch (Exception e)
                {
                    _DateTime = _minWinFileTime;
                    //Console.WriteLine(e.Message);
                    //Console.WriteLine(e.StackTrace);
                    //Console.WriteLine("invalid date "+this.Year+"/"+this.Month+"/"+this.Day+" "+this.Hour+":"+this.Minute+"."+this.Second);
                    //int year = (int)((this._Time & 0xffff) & 0x7f) + 2000;
                    //int month = (int)((this._Time & 0xffff) >> 7) & 0xf;
                    //int day = (int)((this._Time & 0xffff) >> 0xb);
                    //int hour = (int)((this._Time >> 16) & 0x1f);
                    //int minute = (int)((this._Time >> 16) >> 5) & 0x3f;
                    //int second = (int)((this._Time >> 16) >> 10) & 0xfffe;

                    try
                    {
                        _DateTime = _minWinFileTime;
                    }
                    catch (Exception e2)
                    {
                        _DateTime = _minWinFileTime;
                        //Console.WriteLine(e2.Message);
                        //Console.WriteLine(e2.StackTrace);
                        //Console.WriteLine("invalid date "+year+"/"+month+"/"+day+" "+hour+":"+minute+"."+second);
                        //Console.WriteLine("falling back to 1601/01/01");
                        //_DateTime = _minWinFileTime;
                    }

                    return _DateTime.Value;
                }
            }
        }
    }
}
