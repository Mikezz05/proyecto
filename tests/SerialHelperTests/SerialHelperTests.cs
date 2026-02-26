using System;
using Xunit;

namespace SistemaPintoSalinas.Tests
{
    public class SerialHelperTests
    {
        [Fact]
        public void GenerateSerial_LengthAndCharset()
        {
            string s = SistemaPintoSalinas.SerialHelper.GenerateSerial(12);
            Assert.NotNull(s);
            Assert.Equal(12, s.Length);
            // caracteres válidos
            foreach (char c in s) {
                Assert.Contains(c, "ABCDEFGHJKMNPQRSTUVWXYZ23456789");
            }
        }

        [Fact]
        public void HashAndVerifySerial_Works()
        {
            string serial = SistemaPintoSalinas.SerialHelper.GenerateSerial(10);
            string hash = SistemaPintoSalinas.SerialHelper.HashSerial(serial);
            bool ok = SistemaPintoSalinas.SerialHelper.VerifyHashedSerial(hash, serial);
            Assert.True(ok);
            // verificar que otro serial no pasa
            bool ok2 = SistemaPintoSalinas.SerialHelper.VerifyHashedSerial(hash, serial + "X");
            Assert.False(ok2);
        }
    }
}
