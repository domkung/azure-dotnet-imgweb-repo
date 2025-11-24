namespace Web
{
    public class Calculator
    {
        /// <summary>
        /// คืนผลรวมของสองจำนวนเต็ม
        /// </summary>
        /// <param name="a">จำนวนเต็มตัวแรก</param>
        /// <param name="b">จำนวนเต็มตัวที่สอง</param>
        /// <returns>ผลรวมเป็นจำนวนเต็ม</returns>
        public int Add(int a, int b) => a + b;

        // Subtract two integers
        public int Subtract(int a, int b) => a - b;
        // Multiply two integers
        public int Multiply(int a, int b) => a * b;
        // Divide two integers
        public double Divide(int a, int b) => (double)a / b;
        
    }
}