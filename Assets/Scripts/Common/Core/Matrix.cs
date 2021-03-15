public class Matrix<T> 
{
	private T[][] data;
	private int _rows;
	private int _cols;
	
	public Matrix(int rows, int cols)
	{
		data = new T[rows][];
		for (var i = 0; i < rows; i++) {
			data[i] = new T[cols];
		}

		Rows = rows;
		Cols = cols;
	}

	private Matrix(Matrix<T> m)
	{
		data = m.ToArray();
		Rows = m.Cols;
		Cols = m.Rows;
	}

	public int Rows {
		get => _rows;
		set => _rows = value;
	}

	public int Cols {
		get => _cols;
		set => _cols = value;
	}

	public virtual Matrix<T> Transpose()
	{
		return new Transposed(this);
	}

	public virtual T GetElement(int row, int col)
	{
		return data[row][col];
	}

	public virtual void SetElement(int row, int col, T obj)
	{
		data[row][col] = obj;
	}

	public void ReverseColumnsInPlace()
	{
		for (var col = 0; col < Cols; col++) {
			for (var row = 0; row < Rows / 2; row++) {
				var temp = GetElement(row, col);
				SetElement(row, col, GetElement(Rows - row - 1, col));
				SetElement(Rows - row - 1, col, temp);
			}
		}
	}

	public void ReverseRowsInPlace()
	{
		for (var row = 0; row < Rows; row++) {
			for (var col = 0; col < Cols / 2; col++) {
				var temp = GetElement(row, col);
				SetElement(row, col, GetElement(row, Cols - col - 1));
				SetElement(row, Cols - col - 1, temp);
			}
		}
	}

	public T[][] ToArray()
	{
		return data;
	}
	
	public class Transposed : Matrix<T>
	{
		private Matrix<T> original;

		public Transposed(Matrix<T> m) : base(m)
		{
			original = m;
		}

		public override T GetElement(int row, int col)
		{
			return original.GetElement(col, row);
		}

		public override void SetElement(int row, int col, T obj)
		{
			original.SetElement(col, row, obj);
		}

		public override Matrix<T> Transpose()
		{
			return original;
		}
	}
}