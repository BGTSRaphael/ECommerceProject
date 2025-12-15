using Xunit;

namespace ECommerceProject.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test_SimpleAddition_ShouldPass()
        {
            // Arrange
            int a = 2;
            int b = 3;
            int expected = 5;

            // Act
            int actual = a + b;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_StringConcatenation_ShouldPass()
        {
            // Arrange
            string hello = "Hello";
            string world = "World";
            string expected = "Hello World";

            // Act
            string actual = $"{hello} {world}";

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_BooleanTrue_ShouldPass()
        {
            // Arrange & Act
            bool result = true;

            // Assert
            Assert.True(result);
        }
    }
}