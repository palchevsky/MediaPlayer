using System;
using System.Collections;
using System.Collections.Generic;

namespace MediaPlayer.Class
{
    public class FileNames:IEnumerable<FileNames>
    {
        public string ShortName { get; set; }

        public string FullName { get; set; }

        public override string ToString()
        {
            return $"{ShortName}";
        }

        public IEnumerator<FileNames> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public FileNames(string shortN, string fullN)
        {
            ShortName = shortN;
            FullName = fullN;
        }
    }
}
