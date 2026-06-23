using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(Stream filestream, string fileName, string folder);
        void deleteFile(string fileUrl);
    }
}
