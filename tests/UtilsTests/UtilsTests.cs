using System;
using Xunit;
namespace SistemaPintoSalinas.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void CreateHashAndVerify_Pbkdf2_Works()
        {
            string pwd = "TestP@ssw0rd!";
            string hash = SistemaPintoSalinas.Utils.CreatePasswordHash(pwd);
            bool needsUpgrade;
            bool ok = SistemaPintoSalinas.Utils.VerifyPassword(hash, pwd, out needsUpgrade);
            Assert.True(ok);
            Assert.False(needsUpgrade);
            Assert.StartsWith("pbkdf2$", hash);
        }

        [Fact]
        public void LegacySha256_Verify_RequiresUpgrade()
        {
            string pwd = "legacy123";
            string sha = SistemaPintoSalinas.Utils.ComputeSHA256(pwd);
            bool needsUpgrade;
            bool ok = SistemaPintoSalinas.Utils.VerifyPassword(sha, pwd, out needsUpgrade);
            Assert.True(ok);
            Assert.True(needsUpgrade);
        }
    }
}
