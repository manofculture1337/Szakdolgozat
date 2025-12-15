using Assets.Scripts.CleanArchitecture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.CleanArchitecture.Usecases
{
    internal class SendFileUsecase
    {
        private Entities.FileSender fileSender;

        public SendFileUsecase(Entities.FileSender fileSender)
        {
            this.fileSender = fileSender;
        }

        public void SendMesh()
        {
            fileSender.SendMesh();
        }

        public void SendFile(string filePath)
        {
            fileSender.SendFileToAll(filePath);
        }
    }
}
