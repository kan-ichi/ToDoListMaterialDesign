using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoListMaterialDesign.Models.Codes;

namespace ToDoListMaterialDesign.CoreTest.Models.Codes
{
    [TestClass]
    public class StatusCodeTest
    {
        [TestMethod]
        public void Test0010()
        {
            var code = new StatusCode(string.Empty);

            Assert.IsFalse(code.IsNotYet);
            Assert.IsFalse(code.IsFinished);

            Assert.AreEqual(string.Empty, code.Code);
            Assert.AreEqual(string.Empty, code.Name);
        }

        [TestMethod]
        public void Test0020()
        {
            var code = new StatusCode(StatusCode.CODE_NOT_YET);

            Assert.IsTrue(code.IsNotYet);
            Assert.IsFalse(code.IsFinished);

            Assert.AreEqual(StatusCode.CODE_NOT_YET, code.Code);
            Assert.AreEqual(StatusCode.NAME_NOT_YET, code.Name);
        }

        [TestMethod]
        public void Test0030()
        {
            var code = new StatusCode(StatusCode.CODE_FINISHED);

            Assert.IsFalse(code.IsNotYet);
            Assert.IsTrue(code.IsFinished);

            Assert.AreEqual(StatusCode.CODE_FINISHED, code.Code);
            Assert.AreEqual(StatusCode.NAME_FINISHED, code.Name);
        }

        [TestMethod]
        public void Test0040()
        {
            Assert.IsFalse(StatusCode.HasName(string.Empty));
            Assert.IsFalse(StatusCode.HasName(StatusCode.CODE_NOT_YET));
            Assert.IsFalse(StatusCode.HasName(StatusCode.CODE_FINISHED));
            Assert.IsTrue(StatusCode.HasName(StatusCode.NAME_NOT_YET));
            Assert.IsTrue(StatusCode.HasName(StatusCode.NAME_FINISHED));
        }

        [TestMethod]
        public void Test0050()
        {
            Assert.IsNull(StatusCode.GetCodeByName(string.Empty));
            Assert.IsNull(StatusCode.GetCodeByName(StatusCode.CODE_NOT_YET));
            Assert.IsNull(StatusCode.GetCodeByName(StatusCode.CODE_FINISHED));
            Assert.AreEqual(StatusCode.CODE_NOT_YET, StatusCode.GetCodeByName(StatusCode.NAME_NOT_YET));
            Assert.AreEqual(StatusCode.CODE_FINISHED, StatusCode.GetCodeByName(StatusCode.NAME_FINISHED));
        }

        [TestMethod]
        public void Test0060()
        {
            Assert.IsNull(StatusCode.GetNameByCode(string.Empty));
            Assert.IsNull(StatusCode.GetNameByCode(StatusCode.NAME_NOT_YET));
            Assert.IsNull(StatusCode.GetNameByCode(StatusCode.NAME_FINISHED));
            Assert.AreEqual(StatusCode.NAME_NOT_YET, StatusCode.GetNameByCode(StatusCode.CODE_NOT_YET));
            Assert.AreEqual(StatusCode.NAME_FINISHED, StatusCode.GetNameByCode(StatusCode.CODE_FINISHED));
        }
    }
}
