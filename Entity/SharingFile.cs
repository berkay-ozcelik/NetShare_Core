using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetShare_Core.Entity
{
	public class SharingFile
	{
		public string FileName { get; set; }

		[JsonIgnore]
		public string FilePath { get; set; }

		public long FileSize { get; set; }
		public string FileExtension { get; set; }

		[JsonIgnore]
		private FileStream _lock;
		
		

		public SharingFile(FileInfo fileInfo)
		{
			FileName = fileInfo.Name;
			FilePath = fileInfo.FullName;
			FileSize = fileInfo.Length;
			FileExtension = fileInfo.Extension;
		}
		public SharingFile()
		{
			FileName = FilePath = FileExtension = string.Empty;
			FileSize = 0;
		}

		public string Serialize()
		{
			return JsonSerializer.Serialize(this);
		}

		public static SharingFile Deserialize(string json)
		{
			var sharingFile = JsonSerializer.Deserialize<SharingFile>(json);
			return sharingFile;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			var sharingFile = (SharingFile)obj;
			return FileName == sharingFile.FileName &&
				FileSize == sharingFile.FileSize &&
				FileExtension == sharingFile.FileExtension;
		}

		public void LockFile()
		{
			_lock = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public void UnlockFile()
		{
            _lock.Close();
        }

		public override string ToString()
		{
			return $"{FileName} ({FileSize} bytes)";
		}
    }
}

