using System;
using System.Collections.Generic;
using System.IO;

namespace Box2CS
{
	/// <summary>
	/// A Box2CS exclusive.
	/// Mesh shapes are a saved collection of shapes
	/// for use as fixtures.
	/// </summary>
	public class MeshShape
	{
		List<Shape> _shapes = new List<Shape>();
		float _scale = 1;
		bool _invert = false;

		public List<Shape> Shapes
		{
			get { return _shapes; }
		}

		public MeshShape()
		{
		}

		public MeshShape(params Shape[] shapes)
		{
			foreach (var shape in shapes)
				_shapes.Add(shape.Clone());
		}

		public MeshShape(string fileName, float scale, bool invert)
		{
			_invert = invert;
			_scale = scale;
			Load(fileName);
		}

		public MeshShape(Stream stream, bool binary)
		{
			if (binary)
				LoadBinary(stream);
			else
				LoadASCII(stream);
		}

		public void Load(string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				if (fileName.EndsWith(".mesh"))
					LoadASCII(fs);
				else
					LoadBinary(fs);
			}
		}

		public void LoadBinary(Stream stream)
		{
			using (var reader = new BinaryReader(stream))
			{
				ushort count = reader.ReadUInt16();

				for (int i = 0; i < count; ++i)
				{
					ShapeType type = (ShapeType)reader.ReadByte();

					switch (type)
					{
					case ShapeType.Circle:
						{
							CircleShape shape = new CircleShape();
							shape.Position = new Vec2(reader.ReadSingle(), reader.ReadSingle());
							shape.Radius = reader.ReadSingle();
							_shapes.Add(shape);
							continue;
						}
					case ShapeType.Polygon:
						{
							PolygonShape shape = new PolygonShape();
							shape.AutoReverse = true;
							shape.Centroid = new Vec2(reader.ReadSingle(), reader.ReadSingle());
							shape.Radius = reader.ReadSingle();

							byte vertexCount = reader.ReadByte();

							Vec2[] vertices = new Vec2[vertexCount], normals = new Vec2[vertexCount];

							for (int x = (vertexCount - 1); x >= 0; --x)
							{
								vertices[x] = new Vec2(reader.ReadSingle() * _scale, (_invert ? -1 : 1) * (reader.ReadSingle() * _scale));
								normals[x] = new Vec2(reader.ReadSingle(), (_invert ? -1 : 1) * (reader.ReadSingle()));
							}

							shape.Vertices = vertices;
							//shape.Normals = normals;
							_shapes.Add(shape);
						}
						break;
					}
				}
			}
		}

		public void LoadASCII(Stream stream)
		{
		}

		public void Save(string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				if (fileName.EndsWith(".mesh"))
					SaveASCII(fs);
				else
					SaveBinary(fs);
			}
		}

		public void SaveBinary(Stream stream)
		{
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write((ushort)_shapes.Count);

				for (int i = 0; i < _shapes.Count; ++i)
				{
					var shape = _shapes[i];

					writer.Write((byte)shape.ShapeType);

					switch (shape.ShapeType)
					{
					case ShapeType.Circle:
						{
							CircleShape cshape = (CircleShape)shape;
							writer.Write(cshape.Position.X);
							writer.Write(cshape.Position.Y);
							writer.Write(cshape.Radius);
							continue;
						}
					case ShapeType.Polygon:
						{
							PolygonShape cshape = (PolygonShape)shape;
							writer.Write(cshape.Centroid.X);
							writer.Write(cshape.Centroid.Y);
							writer.Write(cshape.Radius);

							writer.Write((byte)cshape.VertexCount);

							for (byte x = 0; x < cshape.VertexCount; ++x)
							{
								writer.Write(cshape.Vertices[x].X);
								writer.Write(cshape.Vertices[x].Y);
								writer.Write(cshape.Normals[x].X);
								writer.Write(cshape.Normals[x].Y);
							}
						}
						break;
					}
				}
			}
		}

		public void SaveASCII(Stream stream)
		{
		}

		public Fixture[] AddToBody(Body body, float density)
		{
			Fixture[] fixtures = new Fixture[_shapes.Count];

			for (int i = 0; i < _shapes.Count; ++i)
				fixtures[i] = body.CreateFixture(_shapes[i], density);

			return fixtures;
		}
	}
}
