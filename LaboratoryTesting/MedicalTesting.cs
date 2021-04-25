using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SessionOne;

namespace LaboratoryTesting
{
    [TestClass]
    public class MedicalTesting
    {
        [TestMethod]
        public void TestSignInPol()
        {
            // arrange
            string login = "chacking0";
            string pass = "4tzqHdkqzo4";

            // act
            //bool result = SessionOne.ViewModel.DBViewModel.LoginUserIn(login, pass);

            // assert
            //Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestSignInNeg()
        {
            // arrange
            string login = "stellar";
            string pass = "sfhsfdg";

            // act
            //bool result = SessionOne.ViewModel.DBViewModel.LoginUserIn(login, pass);

            // assert
            //Assert.AreEqual(false, result);
        }
    }
}
