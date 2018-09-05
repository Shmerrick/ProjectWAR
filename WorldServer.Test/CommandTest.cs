using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.Managers.Commands;

namespace WorldServer.Test
{
    [TestClass]
    public class CommandTest
    {
        [TestMethod]
        public void AreGoCommandsAvailable()
        {
            var commandList = CommandDeclarations.GoCommands;

            Assert.IsTrue(commandList.Count > 0);
        }

        [TestMethod]
        public void AreModifyCommandsAvailable()
        {
            var commandList = CommandDeclarations.ModifyCommands;

            Assert.IsTrue(commandList.Count > 0);
        }


    }
}
