using System;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Data;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers
{
    public class Mp3Helper
    {
        private const int ProgrammVersion = 1;

        public static Mp3FileMetaData GetMp3FileMetaData(Mp3Model model)
        {
            if (model.FileInfo != null)
                return new Mp3FileMetaData()
                {
                    CreateDate = model.FileInfo.CreateDate,
                    CreatedProgramVersion = model.FileInfo.CreatedProgramVersion,
                    SaveDate = DateTime.Now,
                    SaveProgramVersion = ProgrammVersion,
                    Id = model.VideoModel.Id
                };
            return new Mp3FileMetaData()
            {
                CreateDate = DateTime.Now,
                CreatedProgramVersion = ProgrammVersion,
                SaveDate = DateTime.Now,
                SaveProgramVersion = ProgrammVersion,
                Id = model.VideoModel.Id
            };
        }
    }
}
