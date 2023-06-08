using System;
using System.Text.Json;
using NetShare_Core.Entity;

namespace NetShare_Core.Device
{
	public class FileManager
	{
		public static readonly FileManager Instance ;

		static FileManager() { 
			Instance = new FileManager();
			
			
			#if DEBUG
			var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
			var files = currentDirectory.GetFiles();
			foreach (var file in files)
			{
				Instance.AddFile(file);
			}
			#endif
		}


		private List<SharingFile> _sharingFiles;

		private FileManager() {
			_sharingFiles = new List<SharingFile>();
		}

		public void AddFile(FileInfo fileInfo) {
			_sharingFiles.Add(new SharingFile(fileInfo));
		}

		public SharingFile? GetFile(SharingFile key)
		{
			return _sharingFiles.Find(file => file.Equals(key));
		}

		public void RemoveFile(SharingFile sharingFile) {
			_sharingFiles.Remove(sharingFile);
		}

		public string Serialize() {
			return JsonSerializer.Serialize(_sharingFiles);
		}

		public static List<SharingFile> Deserialize(string json) {
			return JsonSerializer.Deserialize<List<SharingFile>>(json);
		}

        public List<SharingFile> SharingFiles
        {
            get => _sharingFiles;
        }
    }
}

