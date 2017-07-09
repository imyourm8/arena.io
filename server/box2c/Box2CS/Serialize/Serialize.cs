//#define SKIP_DEFAULT_CHECKS

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Box2CS.Parsers;

namespace Box2CS.Serialize
{
	public class FixtureDefSerialized
	{
		public FixtureDef Fixture
		{
			get;
			set;
		}

		public int ShapeID
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public FixtureDefSerialized(FixtureDef fixture, int shapeID, string name)
		{
			Fixture = fixture;
			ShapeID = shapeID;
			Name = name;
		}
	}

	public class BodyDefSerialized
	{
		public BodyDef Body
		{
			get;
			set;
		}

		public List<int> FixtureIDs
		{
			get;
			set;
		}

		public Body DerivedBody
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public BodyDefSerialized(Body derivedBody, BodyDef body, List<int> fixtureIDs, string name)
		{
			DerivedBody = derivedBody;
			Body = body;
			FixtureIDs = fixtureIDs;
			Name = name;
		}
	}

	public class JointDefSerialized
	{
		public JointDef Joint
		{
			get;
			set;
		}

		public int BodyAIndex
		{
			get;
			set;
		}

		public int BodyBIndex
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public JointDefSerialized(JointDef joint, int bodyA, int bodyB, string name)
		{
			Joint = joint;
			BodyAIndex = bodyA;
			BodyBIndex = bodyB;
			Name = name;
		}
	}

	public class ShapeSerialized
	{
		public Shape Shape
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public ShapeSerialized(Shape shape, string name)
		{
			Shape = shape;
			Name = name;
		}
	}

	public interface IWorldSerializer
	{
		void Open(Stream stream, IWorldSerializationProvider provider);
		void Close();

		void BeginSerializingShapes();
		void EndSerializingShapes();

		void BeginSerializingFixtures();
		void EndSerializingFixtures();

		void BeginSerializingBodies();
		void EndSerializingBodies();

		void SerializeShape(ShapeSerialized shape);
		void SerializeFixture(FixtureDefSerialized fixture);
		void SerializeBody(BodyDefSerialized body);

		void BeginSerializingJoints();
		void EndSerializingJoints();

		void SerializeJoint(JointDefSerialized joint);
	}

	public interface IWorldSerializationProvider
	{
		IList<ShapeSerialized> Shapes
		{
			get;
		}

		IList<FixtureDefSerialized> FixtureDefs
		{
			get;
		}

		int IndexOfFixture(FixtureDef def);

		IList<BodyDefSerialized> Bodies
		{
			get;
		}

		int IndexOfBody(BodyDef def);

		IList<JointDefSerialized> Joints
		{
			get;
		}

		World World
		{
			get;
		}
	}

	public interface IWorldDeserializer
	{
		WorldData Deserialize(Stream stream);
	}

	public class WorldXmlSerializer : IWorldSerializer
	{
		XmlWriter writer;
		IWorldSerializationProvider _provider;

		public const int XmlVersion = 1;

		void WriteEndElement()
		{
			writer.WriteEndElement();
		}

		void WriteSimpleType(Type type, object val)
		{
			var serializer = new XmlSerializerFactory().CreateSerializer(type);
			XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
			xmlnsEmpty.Add("", "");
			serializer.Serialize(writer, val, xmlnsEmpty);
		}

		void WriteDynamicType(Type type, object val)
		{
			writer.WriteElementString("Type", type.FullName);

			writer.WriteStartElement("Value");
			var serializer = new XmlSerializerFactory().CreateSerializer(type);
			XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
			xmlnsEmpty.Add("", "");
			serializer.Serialize(writer, val, xmlnsEmpty);
			writer.WriteEndElement();
		}

		void WriteElement(string name, int val)
		{
			writer.WriteElementString(name, val.ToString());
		}

		void WriteElement(string name, float val)
		{
			writer.WriteElementString(name, val.ToString());
		}

		void WriteElement(string name, double val)
		{
			writer.WriteElementString(name, val.ToString());
		}

		void WriteElement(string name, bool val)
		{
			writer.WriteElementString(name, val.ToString());
		}

		void WriteElement(string name, Vec2 vec)
		{
			/*writer.WriteStartElement(name);
			writer.WriteAttributeString("X", vec.X.ToString());
			writer.WriteAttributeString("Y", vec.Y.ToString());
			WriteEndElement();*/
			writer.WriteElementString(name, vec.X.ToString() + " " + vec.Y.ToString());
		}

		public void Open(Stream stream, IWorldSerializationProvider provider)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;
			settings.OmitXmlDeclaration = true;

			writer = XmlWriter.Create(stream, settings);

			writer.WriteStartElement("World");

			writer.WriteAttributeString("Version", XmlVersion.ToString());

			WriteElement("Gravity", provider.World.Gravity);

			_provider = provider;
		}

		public void Close()
		{
			WriteEndElement();

			writer.Flush();
			writer.Close();
			writer = null;
		}

		public void BeginSerializingShapes()
		{
			writer.WriteStartElement("Shapes");
		}

		public void EndSerializingShapes()
		{
			WriteEndElement();
		}

		public void BeginSerializingFixtures()
		{
			writer.WriteStartElement("Fixtures");
		}

		public void EndSerializingFixtures()
		{
			WriteEndElement();
		}

		public void BeginSerializingBodies()
		{
			writer.WriteStartElement("Bodies");
		}

		public void EndSerializingBodies()
		{
			WriteEndElement();
		}

		public void SerializeShape(ShapeSerialized shape)
		{
			writer.WriteStartElement("Shape");
			writer.WriteAttributeString("Type", shape.Shape.ShapeType.ToString());

			if (!string.IsNullOrEmpty(shape.Name))
				writer.WriteElementString("Name", shape.Name);

			switch (shape.Shape.ShapeType)
			{
			case ShapeType.Circle:
				{
					CircleShape circle = (CircleShape)shape.Shape;

					writer.WriteElementString("Radius", circle.Radius.ToString());

					WriteElement("Position", circle.Position);
				}
				break;
			case ShapeType.Polygon:
				{
					PolygonShape poly = (PolygonShape)shape.Shape;

					writer.WriteStartElement("Vertices");
					foreach (var v in poly.Vertices)
						WriteElement("Vertex", v);
					WriteEndElement();

					WriteElement("Centroid", poly.Centroid);
				}
				break;
			default:
				throw new Exception();
			}

			WriteEndElement();
		}

		static FixtureDef defaultFixtureDefData = new FixtureDef();

		public void SerializeFixture(FixtureDefSerialized fixture)
		{
			writer.WriteStartElement("Fixture");

			writer.WriteElementString("Shape", fixture.ShapeID.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (!string.IsNullOrEmpty(fixture.Name))
#endif
				writer.WriteElementString("Name", fixture.Name);

#if !SKIP_DEFAULT_CHECKS
			if (fixture.Fixture.Density != defaultFixtureDefData.Density)
#endif
				writer.WriteElementString("Density", fixture.Fixture.Density.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (fixture.Fixture.Filter != defaultFixtureDefData.Filter)
#endif
				WriteSimpleType(typeof(FilterData), fixture.Fixture.Filter);

#if !SKIP_DEFAULT_CHECKS
			if (fixture.Fixture.Friction != defaultFixtureDefData.Friction)
#endif
				writer.WriteElementString("Friction", fixture.Fixture.Friction.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (fixture.Fixture.IsSensor != defaultFixtureDefData.IsSensor)
#endif
				writer.WriteElementString("IsSensor", fixture.Fixture.IsSensor.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (fixture.Fixture.Restitution != defaultFixtureDefData.Restitution)
#endif
				writer.WriteElementString("Restitution", fixture.Fixture.Restitution.ToString());

			if (fixture.Fixture.UserData != null)
			{
				writer.WriteStartElement("UserData");
				WriteDynamicType(fixture.Fixture.UserData.GetType(), fixture.Fixture.UserData);
				WriteEndElement();
			}

			WriteEndElement();
		}

		static BodyDef defaultBodyDefData = new BodyDef();

		public void SerializeBody(BodyDefSerialized body)
		{
			writer.WriteStartElement("Body");
			writer.WriteAttributeString("Type", body.Body.BodyType.ToString());

			if (!string.IsNullOrEmpty(body.Name))
				writer.WriteElementString("Name", body.Name);

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.Active != defaultBodyDefData.Active)
#endif
				writer.WriteElementString("Active", body.Body.Active.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.AllowSleep != defaultBodyDefData.AllowSleep)
#endif
				writer.WriteElementString("AllowSleep", body.Body.AllowSleep.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.Angle != defaultBodyDefData.Angle)
#endif
				writer.WriteElementString("Angle", body.Body.Angle.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.AngularDamping != defaultBodyDefData.AngularDamping)
#endif
				writer.WriteElementString("AngularDamping", body.Body.AngularDamping.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.AngularVelocity != defaultBodyDefData.AngularVelocity)
#endif
				writer.WriteElementString("AngularVelocity", body.Body.AngularVelocity.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.Awake != defaultBodyDefData.Awake)
#endif
				writer.WriteElementString("Awake", body.Body.Awake.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.Bullet != defaultBodyDefData.Bullet)
#endif
				writer.WriteElementString("Bullet", body.Body.Bullet.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.FixedRotation != defaultBodyDefData.FixedRotation)
#endif
				writer.WriteElementString("FixedRotation", body.Body.FixedRotation.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.InertiaScale != defaultBodyDefData.InertiaScale)
#endif
				writer.WriteElementString("InertiaScale", body.Body.InertiaScale.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.LinearDamping != defaultBodyDefData.LinearDamping)
#endif
				writer.WriteElementString("LinearDamping", body.Body.LinearDamping.ToString());

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.LinearVelocity != defaultBodyDefData.LinearVelocity)
#endif
				WriteElement("LinearVelocity", body.Body.LinearVelocity);

#if !SKIP_DEFAULT_CHECKS
			if (body.Body.Position != defaultBodyDefData.Position)
#endif
				WriteElement("Position", body.Body.Position);

			if (body.Body.UserData != null)
			{
				writer.WriteStartElement("UserData");
				WriteDynamicType(body.Body.UserData.GetType(), body.Body.UserData);
				WriteEndElement();
			}

			writer.WriteStartElement("Fixtures");
			foreach (var fixture in body.FixtureIDs)
				writer.WriteElementString("ID", fixture.ToString());
			WriteEndElement();

			WriteEndElement();
		}

		public void BeginSerializingJoints()
		{
			writer.WriteStartElement("Joints");
		}

		public void EndSerializingJoints()
		{
			writer.WriteEndElement();
		}

		static DistanceJointDef _defaultDistanceJoint = new DistanceJointDef();
		static FrictionJointDef _defaultFrictionJoint = new FrictionJointDef();
		static LineJointDef _defaultLineJoint = new LineJointDef();
		static PrismaticJointDef _defaultPrismaticJoint = new PrismaticJointDef();
		static PulleyJointDef _defaultPulleyJoint = new PulleyJointDef();
		static RevoluteJointDef _defaultRevoluteJoint = new RevoluteJointDef();
		static WeldJointDef _defaultWeldJoint = new WeldJointDef();

		public void SerializeJoint(JointDefSerialized def)
		{
			writer.WriteStartElement("Joint");

			writer.WriteAttributeString("Type", def.Joint.JointType.ToString());

			if (!string.IsNullOrEmpty(def.Name))
				writer.WriteElementString("Name", def.Name);

			WriteElement("BodyA", def.BodyAIndex);
			WriteElement("BodyB", def.BodyBIndex);

#if !SKIP_DEFAULT_CHECKS
			if (def.Joint.CollideConnected != false)
#endif
				WriteElement("CollideConnected", def.Joint.CollideConnected);

			if (def.Joint.UserData != null)
			{
				writer.WriteStartElement("UserData");
				WriteDynamicType(def.Joint.UserData.GetType(), def.Joint.UserData);
				WriteEndElement();
			}

			switch (def.Joint.JointType)
			{
			case JointType.Distance:
				{
					DistanceJointDef djd = (DistanceJointDef)def.Joint;

#if !SKIP_DEFAULT_CHECKS
					if (djd.DampingRatio != _defaultDistanceJoint.DampingRatio)
#endif
						WriteElement("DampingRatio", djd.DampingRatio);
#if !SKIP_DEFAULT_CHECKS
					if (djd.FrequencyHz != _defaultDistanceJoint.FrequencyHz)
#endif
						WriteElement("FrequencyHz", djd.FrequencyHz);
#if !SKIP_DEFAULT_CHECKS
					if (djd.Length != _defaultDistanceJoint.Length)
#endif
						WriteElement("Length", djd.Length);
#if !SKIP_DEFAULT_CHECKS
					if (djd.LocalAnchorA != _defaultDistanceJoint.LocalAnchorA)
#endif
						WriteElement("LocalAnchorA", djd.LocalAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (djd.LocalAnchorB != _defaultDistanceJoint.LocalAnchorB)
#endif
						WriteElement("LocalAnchorB", djd.LocalAnchorB);
				}
				break;
			case JointType.Friction:
				{
					FrictionJointDef fjd = (FrictionJointDef)def.Joint;

#if !SKIP_DEFAULT_CHECKS
					if (fjd.LocalAnchorA != _defaultFrictionJoint.LocalAnchorA)
#endif
						WriteElement("LocalAnchorA", fjd.LocalAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (fjd.LocalAnchorB != _defaultFrictionJoint.LocalAnchorB)
#endif
						WriteElement("LocalAnchorB", fjd.LocalAnchorB);
#if !SKIP_DEFAULT_CHECKS
					if (fjd.MaxForce != _defaultFrictionJoint.MaxForce)
#endif
						WriteElement("MaxForce", fjd.MaxForce);
#if !SKIP_DEFAULT_CHECKS
					if (fjd.MaxTorque != _defaultFrictionJoint.MaxTorque)
#endif
						WriteElement("MaxTorque", fjd.MaxTorque);
				}
				break;
			case JointType.Gear:
				/*	GearJointDef gjd = (GearJointDef)def.Joint;

					int jointA = -1, jointB = -1;

					for (int i = 0; i < _provider.Joints.Count; ++i)
					{
						if (gjd.JointA == _provider.Joints[i].DerivedJoint)
							jointA = i;

						if (gjd.JointB == _provider.Joints[i].DerivedJoint)
							jointB = i;

						if (jointA != -1 && jointB != -1)
							break;
					}

					WriteElement("JointA", jointA);
					WriteElement("JointB", jointB);
					WriteElement("Ratio", gjd.Ratio);*/
				throw new Exception("Gear joint not supported by serialization");
			case JointType.Line:
				{
					LineJointDef ljd = (LineJointDef)def.Joint;

#if !SKIP_DEFAULT_CHECKS
					if (ljd.EnableLimit != _defaultLineJoint.EnableLimit)
#endif
						WriteElement("EnableLimit", ljd.EnableLimit);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.EnableMotor != _defaultLineJoint.EnableMotor)
#endif
						WriteElement("EnableMotor", ljd.EnableMotor);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.LocalAnchorA != _defaultLineJoint.LocalAnchorA)
#endif
						WriteElement("LocalAnchorA", ljd.LocalAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.LocalAnchorB != _defaultLineJoint.LocalAnchorB)
#endif
						WriteElement("LocalAnchorB", ljd.LocalAnchorB);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.LocalAxisA != _defaultLineJoint.LocalAxisA)
#endif
						WriteElement("LocalAxisA", ljd.LocalAxisA);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.LowerTranslation != _defaultLineJoint.LowerTranslation)
#endif
						WriteElement("LowerTranslation", ljd.LowerTranslation);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.MaxMotorForce != _defaultLineJoint.MaxMotorForce)
#endif
						WriteElement("MaxMotorForce", ljd.MaxMotorForce);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.MotorSpeed != _defaultLineJoint.MotorSpeed)
#endif
						WriteElement("MotorSpeed", ljd.MotorSpeed);
#if !SKIP_DEFAULT_CHECKS
					if (ljd.UpperTranslation != _defaultLineJoint.UpperTranslation)
#endif
						WriteElement("UpperTranslation", ljd.UpperTranslation);
				}
				break;
			case JointType.Prismatic:
				{
					PrismaticJointDef pjd = (PrismaticJointDef)def.Joint;

#if !SKIP_DEFAULT_CHECKS
					if (pjd.EnableLimit != _defaultPrismaticJoint.EnableLimit)
#endif
						WriteElement("EnableLimit", pjd.EnableLimit);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.EnableMotor != _defaultPrismaticJoint.EnableMotor)
#endif
						WriteElement("EnableMotor", pjd.EnableMotor);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LocalAnchorA != _defaultPrismaticJoint.LocalAnchorA)
#endif
						WriteElement("LocalAnchorA", pjd.LocalAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LocalAnchorB != _defaultPrismaticJoint.LocalAnchorB)
#endif
						WriteElement("LocalAnchorB", pjd.LocalAnchorB);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LocalAxis != _defaultPrismaticJoint.LocalAxis)
#endif
						WriteElement("LocalAxisA", pjd.LocalAxis);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LowerTranslation != _defaultPrismaticJoint.LowerTranslation)
#endif
						WriteElement("LowerTranslation", pjd.LowerTranslation);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.MaxMotorForce != _defaultPrismaticJoint.MaxMotorForce)
#endif
						WriteElement("MaxMotorForce", pjd.MaxMotorForce);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.MotorSpeed != _defaultPrismaticJoint.MotorSpeed)
#endif
						WriteElement("MotorSpeed", pjd.MotorSpeed);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.UpperTranslation != _defaultPrismaticJoint.UpperTranslation)
#endif
						WriteElement("UpperTranslation", pjd.UpperTranslation);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.ReferenceAngle != _defaultPrismaticJoint.ReferenceAngle)
#endif
						WriteElement("ReferenceAngle", pjd.ReferenceAngle);
				}
				break;
			case JointType.Pulley:
				{
					PulleyJointDef pjd = (PulleyJointDef)def.Joint;

#if !SKIP_DEFAULT_CHECKS
					if (pjd.GroundAnchorA != _defaultPulleyJoint.GroundAnchorA)
#endif
						WriteElement("GroundAnchorA", pjd.GroundAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.GroundAnchorB != _defaultPulleyJoint.GroundAnchorB)
#endif
						WriteElement("GroundAnchorB", pjd.GroundAnchorB);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LengthA != _defaultPulleyJoint.LengthA)
#endif
						WriteElement("LengthA", pjd.LengthA);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LengthB != _defaultPulleyJoint.LengthB)
#endif
						WriteElement("LengthB", pjd.LengthB);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LocalAnchorA != _defaultPulleyJoint.LocalAnchorA)
#endif
						WriteElement("LocalAnchorA", pjd.LocalAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.LocalAnchorB != _defaultPulleyJoint.LocalAnchorB)
#endif
						WriteElement("LocalAnchorB", pjd.LocalAnchorB);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.MaxLengthA != _defaultPulleyJoint.MaxLengthA)
#endif
						WriteElement("MaxLengthA", pjd.MaxLengthA);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.MaxLengthB != _defaultPulleyJoint.MaxLengthB)
#endif
						WriteElement("MaxLengthB", pjd.MaxLengthB);
#if !SKIP_DEFAULT_CHECKS
					if (pjd.Ratio != _defaultPulleyJoint.Ratio)
#endif
						WriteElement("Ratio", pjd.Ratio);
				}
				break;
			case JointType.Revolute:
				{
					RevoluteJointDef rjd = (RevoluteJointDef)def.Joint;

#if !SKIP_DEFAULT_CHECKS
					if (rjd.EnableLimit != _defaultRevoluteJoint.EnableLimit)
#endif
						WriteElement("EnableLimit", rjd.EnableLimit);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.EnableMotor != _defaultRevoluteJoint.EnableMotor)
#endif
						WriteElement("EnableMotor", rjd.EnableMotor);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.LocalAnchorA != _defaultRevoluteJoint.LocalAnchorA)
#endif
						WriteElement("LocalAnchorA", rjd.LocalAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.LocalAnchorB != _defaultRevoluteJoint.LocalAnchorB)
#endif
						WriteElement("LocalAnchorB", rjd.LocalAnchorB);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.LowerAngle != _defaultRevoluteJoint.LowerAngle)
#endif
						WriteElement("LowerAngle", rjd.LowerAngle);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.MaxMotorTorque != _defaultRevoluteJoint.MaxMotorTorque)
#endif
						WriteElement("MaxMotorTorque", rjd.MaxMotorTorque);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.MotorSpeed != _defaultRevoluteJoint.MotorSpeed)
#endif
						WriteElement("MotorSpeed", rjd.MotorSpeed);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.ReferenceAngle != _defaultRevoluteJoint.ReferenceAngle)
#endif
						WriteElement("ReferenceAngle", rjd.ReferenceAngle);
#if !SKIP_DEFAULT_CHECKS
					if (rjd.UpperAngle != _defaultRevoluteJoint.UpperAngle)
#endif
						WriteElement("UpperAngle", rjd.UpperAngle);
				}
				break;
			case JointType.Weld:
				{
					WeldJointDef wjd = (WeldJointDef)def.Joint;

#if !SKIP_DEFAULT_CHECKS
					if (wjd.LocalAnchorA != _defaultWeldJoint.LocalAnchorA)
#endif
						WriteElement("LocalAnchorA", wjd.LocalAnchorA);
#if !SKIP_DEFAULT_CHECKS
					if (wjd.LocalAnchorB != _defaultWeldJoint.LocalAnchorB)
#endif
						WriteElement("LocalAnchorB", wjd.LocalAnchorB);
				}
				break;
			default:
				throw new Exception();
			}

			writer.WriteEndElement();
		}
	}

	public class WorldData
	{
		public int Version
		{
			get;
			set;
		}

		public Vec2 Gravity
		{
			get;
			set;
		}
	}

	public class WorldXmlDeserializer : IWorldDeserializer, IWorldSerializationProvider
	{
		List<ShapeSerialized> _shapes = new List<ShapeSerialized>();
		List<FixtureDefSerialized> _fixtures = new List<FixtureDefSerialized>();
		List<BodyDefSerialized> _bodies = new List<BodyDefSerialized>();
		List<JointDefSerialized> _joints = new List<JointDefSerialized>();

		public IList<ShapeSerialized> Shapes
		{
			get { return _shapes; }
		}

		public IList<FixtureDefSerialized> FixtureDefs
		{
			get { return _fixtures; }
		}

		public IList<BodyDefSerialized> Bodies
		{
			get { return _bodies; }
		}

		public IList<JointDefSerialized> Joints
		{
			get { return _joints; }
		}

		public World World
		{
			get { return null; }
		}

		public int IndexOfFixture(FixtureDef def)
		{
			for (int i = 0; i < _fixtures.Count; ++i)
				if (_fixtures[i].Fixture == def)
					return i;

			return -1;
		}

		public int IndexOfBody(BodyDef def)
		{
			for (int i = 0; i < _bodies.Count; ++i)
				if (_bodies[i].Body == def)
					return i;

			return -1;
		}

		Vec2 ReadVector(XMLFragmentElement node)
		{
			return Vec2.Parse(node.Value);
		}

		object ReadSimpleType(XMLFragmentElement node, Type type, bool outer)
		{
			if (type == null)
				return ReadSimpleType(node.Elements[1], Type.GetType(node.Elements[0].Value), outer);

			var serializer = new XmlSerializer(type);
			XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
			xmlnsEmpty.Add("", "");

			using (MemoryStream stream = new MemoryStream())
			{
				StreamWriter writer = new StreamWriter(stream);
				{
					writer.Write((outer) ? node.OuterXml : node.InnerXml);
					writer.Flush();
					stream.Position = 0;
				}
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.ConformanceLevel = ConformanceLevel.Fragment;

				return serializer.Deserialize(XmlReader.Create(stream, settings));
			}
		}

		public WorldData Deserialize(Stream stream)
		{
			XMLFragmentElement root = XMLFragmentParser.LoadFromStream(stream);

			if (root.Name.ToLower() != "world")
				throw new Exception();

			WorldData data = new WorldData();

			if (root.Attributes.Count == 0)
				throw new Exception("No version");
			else if (int.Parse(root.Attributes[0].Value) != WorldXmlSerializer.XmlVersion)
				throw new Exception("Wrong version XML file");

			data.Version = int.Parse(root.Attributes[0].Value);

			foreach (var main in root.Elements)
			{
				switch (main.Name.ToLower())
				{
				case "gravity":
					{
						data.Gravity = ReadVector(main);
					}
					break;

				case "shapes":
					{
						foreach (var n in main.Elements)
						{
							if (n.Name.ToLower() != "shape")
								throw new Exception();

							ShapeType type = (ShapeType)Enum.Parse(typeof(ShapeType), n.Attributes[0].Value, true);
							string name = "";

							switch (type)
							{
							case ShapeType.Circle:
								{
									CircleShape shape = new CircleShape();

									foreach (var sn in n.Elements)
									{
										switch (sn.Name.ToLower())
										{
										case "name":
											name = sn.Value;
											break;
										case "radius":
											shape.Radius = float.Parse(sn.Value);
											break;
										case "position":
											shape.Position = ReadVector(sn);
											break;
										default:
											throw new Exception();
										}
									}

									_shapes.Add(new ShapeSerialized(shape, name));
								}
								break;
							case ShapeType.Polygon:
								{
									PolygonShape shape = new PolygonShape();

									foreach (var sn in n.Elements)
									{
										switch (sn.Name.ToLower())
										{
										case "name":
											name = sn.Value;
											break;
										case "vertices":
											{
												List<Vec2> verts = new List<Vec2>();

												foreach (var vert in sn.Elements)
													verts.Add(ReadVector(vert));

												shape.Vertices = verts.ToArray();
											}
											break;
										case "centroid":
											shape.Centroid = ReadVector(sn);
											break;
										}
									}

									_shapes.Add(new ShapeSerialized(shape, name));
								}
								break;
							}
						}
					}
					break;
				case "fixtures":
					{
						foreach (var n in main.Elements)
						{
							FixtureDef fixture = new FixtureDef();

							if (n.Name.ToLower() != "fixture")
								throw new Exception();

							string name = "";
							int id = 0;

							foreach (var sn in n.Elements)
							{
								switch (sn.Name.ToLower())
								{
								case "name":
									name = sn.Value;
									break;
								case "shape":
									id = int.Parse(sn.Value);
									break;
								case "density":
									fixture.Density = float.Parse(sn.Value);
									break;
								case "filterdata":
									fixture.Filter = (FilterData)ReadSimpleType(sn, typeof(FilterData), true);
									break;
								case "friction":
									fixture.Friction = float.Parse(sn.Value);
									break;
								case "issensor":
									fixture.IsSensor = bool.Parse(sn.Value);
									break;
								case "restitution":
									fixture.Restitution = float.Parse(sn.Value);
									break;
								case "userdata":
									fixture.UserData = ReadSimpleType(sn, null, false);
									break;
								}
							}

							fixture.Shape = _shapes[id].Shape;

							_fixtures.Add(new FixtureDefSerialized(fixture, id, name));
						}
					}
					break;
				case "bodies":
					{
						foreach (var n in main.Elements)
						{
							BodyDef body = new BodyDef();

							if (n.Name.ToLower() != "body")
								throw new Exception();

							body.BodyType = (BodyType)Enum.Parse(typeof(BodyType), n.Attributes[0].Value, true);
							List<int> fixtures = new List<int>();
							string name = "";

							foreach (var sn in n.Elements)
							{
								switch (sn.Name.ToLower())
								{
								case "name":
									name = sn.Value;
									break;
								case "active":
									body.Active = bool.Parse(sn.Value);
									break;
								case "allowsleep":
									body.AllowSleep = bool.Parse(sn.Value);
									break;
								case "angle":
									body.Angle = float.Parse(sn.Value);
									break;
								case "angulardamping":
									body.AngularDamping = float.Parse(sn.Value);
									break;
								case "angularvelocity":
									body.AngularVelocity = float.Parse(sn.Value);
									break;
								case "awake":
									body.Awake = bool.Parse(sn.Value);
									break;
								case "bullet":
									body.Bullet = bool.Parse(sn.Value);
									break;
								case "fixedrotation":
									body.FixedRotation = bool.Parse(sn.Value);
									break;
								case "inertiascale":
									body.InertiaScale = float.Parse(sn.Value);
									break;
								case "lineardamping":
									body.LinearDamping = float.Parse(sn.Value);
									break;
								case "linearvelocity":
									body.LinearVelocity = ReadVector(sn);
									break;
								case "position":
									body.Position = ReadVector(sn);
									break;
								case "userdata":
									body.UserData = ReadSimpleType(sn, null, false);
									break;
								case "fixtures":
									{
										foreach (var v in sn.Elements)
											fixtures.Add(int.Parse(v.Value));
										break;
									}
								}
							}

							_bodies.Add(new BodyDefSerialized(null, body, fixtures, name));
						}
					}
					break;
				case "joints":
					{
						foreach (var n in main.Elements)
						{
							JointDef mainDef = null;

							if (n.Name.ToLower() != "joint")
								throw new Exception();

							JointType type = (JointType)Enum.Parse(typeof(JointType), n.Attributes[0].Value, true);

							int bodyA = -1, bodyB = -1;
							bool collideConnected = false;
							object userData = null;
							string name = "";

							switch (type)
							{
							case JointType.Distance:
								mainDef = new DistanceJointDef();
								break;
							case JointType.Friction:
								mainDef = new FrictionJointDef();
								break;
							case JointType.Line:
								mainDef = new LineJointDef();
								break;
							case JointType.Prismatic:
								mainDef = new PrismaticJointDef();
								break;
							case JointType.Pulley:
								mainDef = new PulleyJointDef();
								break;
							case JointType.Revolute:
								mainDef = new RevoluteJointDef();
								break;
							case JointType.Weld:
								mainDef = new WeldJointDef();
								break;
							default:
								throw new Exception("Invalid or unsupported joint");
							}

							foreach (var sn in n.Elements)
							{
								// check for specific nodes
								switch (type)
								{
								case JointType.Distance:
									{
										switch (sn.Name.ToLower())
										{
										case "dampingratio":
											((DistanceJointDef)mainDef).DampingRatio = float.Parse(sn.Value);
											break;
										case "frequencyhz":
											((DistanceJointDef)mainDef).FrequencyHz = float.Parse(sn.Value);
											break;
										case "length":
											((DistanceJointDef)mainDef).Length = float.Parse(sn.Value);
											break;
										case "localanchora":
											((DistanceJointDef)mainDef).LocalAnchorA = ReadVector(sn);
											break;
										case "localanchorb":
											((DistanceJointDef)mainDef).LocalAnchorB = ReadVector(sn);
											break;
										}
									}
									break;
								case JointType.Friction:
									{
										switch (sn.Name.ToLower())
										{
										case "localanchora":
											((FrictionJointDef)mainDef).LocalAnchorA = ReadVector(sn);
											break;
										case "localanchorb":
											((FrictionJointDef)mainDef).LocalAnchorB = ReadVector(sn);
											break;
										case "maxforce":
											((FrictionJointDef)mainDef).MaxForce = float.Parse(sn.Value);
											break;
										case "maxtorque":
											((FrictionJointDef)mainDef).MaxTorque = float.Parse(sn.Value);
											break;
										}
									}
									break;
								case JointType.Line:
									{
										switch (sn.Name.ToLower())
										{
										case "enablelimit":
											((LineJointDef)mainDef).EnableLimit = bool.Parse(sn.Value);
											break;
										case "enablemotor":
											((LineJointDef)mainDef).EnableMotor = bool.Parse(sn.Value);
											break;
										case "localanchora":
											((LineJointDef)mainDef).LocalAnchorA = ReadVector(sn);
											break;
										case "localanchorb":
											((LineJointDef)mainDef).LocalAnchorB = ReadVector(sn);
											break;
										case "localaxisa":
											((LineJointDef)mainDef).LocalAxisA = ReadVector(sn);
											break;
										case "maxmotorforce":
											((LineJointDef)mainDef).MaxMotorForce = float.Parse(sn.Value);
											break;
										case "motorspeed":
											((LineJointDef)mainDef).MotorSpeed = float.Parse(sn.Value);
											break;
										case "lowertranslation":
											((LineJointDef)mainDef).LowerTranslation = float.Parse(sn.Value);
											break;
										case "uppertranslation":
											((LineJointDef)mainDef).UpperTranslation = float.Parse(sn.Value);
											break;
										}
									}
									break;
								case JointType.Prismatic:
									{
										switch (sn.Name.ToLower())
										{
										case "enablelimit":
											((PrismaticJointDef)mainDef).EnableLimit = bool.Parse(sn.Value);
											break;
										case "enablemotor":
											((PrismaticJointDef)mainDef).EnableMotor = bool.Parse(sn.Value);
											break;
										case "localanchora":
											((PrismaticJointDef)mainDef).LocalAnchorA = ReadVector(sn);
											break;
										case "localanchorb":
											((PrismaticJointDef)mainDef).LocalAnchorB = ReadVector(sn);
											break;
										case "localaxisa":
											((PrismaticJointDef)mainDef).LocalAxis = ReadVector(sn);
											break;
										case "maxmotorforce":
											((PrismaticJointDef)mainDef).MaxMotorForce = float.Parse(sn.Value);
											break;
										case "motorspeed":
											((PrismaticJointDef)mainDef).MotorSpeed = float.Parse(sn.Value);
											break;
										case "lowertranslation":
											((PrismaticJointDef)mainDef).LowerTranslation = float.Parse(sn.Value);
											break;
										case "uppertranslation":
											((PrismaticJointDef)mainDef).UpperTranslation = float.Parse(sn.Value);
											break;
										case "referenceangle":
											((PrismaticJointDef)mainDef).ReferenceAngle = float.Parse(sn.Value);
											break;
										}
									}
									break;
								case JointType.Pulley:
									{
										switch (sn.Name.ToLower())
										{
										case "groundanchora":
											((PulleyJointDef)mainDef).GroundAnchorA = ReadVector(sn);
											break;
										case "groundanchorb":
											((PulleyJointDef)mainDef).GroundAnchorB = ReadVector(sn);
											break;
										case "lengtha":
											((PulleyJointDef)mainDef).LengthA = float.Parse(sn.Value);
											break;
										case "lengthb":
											((PulleyJointDef)mainDef).LengthB = float.Parse(sn.Value);
											break;
										case "localanchora":
											((PulleyJointDef)mainDef).LocalAnchorA = ReadVector(sn);
											break;
										case "localanchorb":
											((PulleyJointDef)mainDef).LocalAnchorB = ReadVector(sn);
											break;
										case "maxlengtha":
											((PulleyJointDef)mainDef).MaxLengthA = float.Parse(sn.Value);
											break;
										case "maxlengthb":
											((PulleyJointDef)mainDef).MaxLengthB = float.Parse(sn.Value);
											break;
										case "ratio":
											((PulleyJointDef)mainDef).Ratio = float.Parse(sn.Value);
											break;
										}
									}
									break;
								case JointType.Revolute:
									{
										switch (sn.Name.ToLower())
										{
										case "enablelimit":
											((RevoluteJointDef)mainDef).EnableLimit = bool.Parse(sn.Value);
											break;
										case "enablemotor":
											((RevoluteJointDef)mainDef).EnableMotor = bool.Parse(sn.Value);
											break;
										case "localanchora":
											((RevoluteJointDef)mainDef).LocalAnchorA = ReadVector(sn);
											break;
										case "localanchorb":
											((RevoluteJointDef)mainDef).LocalAnchorB = ReadVector(sn);
											break;
										case "maxmotortorque":
											((RevoluteJointDef)mainDef).MaxMotorTorque = float.Parse(sn.Value);
											break;
										case "motorspeed":
											((RevoluteJointDef)mainDef).MotorSpeed = float.Parse(sn.Value);
											break;
										case "lowerangle":
											((RevoluteJointDef)mainDef).LowerAngle = float.Parse(sn.Value);
											break;
										case "upperangle":
											((RevoluteJointDef)mainDef).UpperAngle = float.Parse(sn.Value);
											break;
										case "referenceangle":
											((RevoluteJointDef)mainDef).ReferenceAngle = float.Parse(sn.Value);
											break;
										}
									}
									break;
								case JointType.Weld:
									{
										switch (sn.Name.ToLower())
										{
										case "localanchora":
											((WeldJointDef)mainDef).LocalAnchorA = ReadVector(sn);
											break;
										case "localanchorb":
											((WeldJointDef)mainDef).LocalAnchorB = ReadVector(sn);
											break;
										}
									}
									break;
								case JointType.Gear:
									throw new Exception("Gear joint is unsupported");
								}

								switch (sn.Name.ToLower())
								{
								case "name":
									name = sn.Value;
									break;
								case "bodya":
									bodyA = int.Parse(sn.Value);
									break;
								case "bodyb":
									bodyB = int.Parse(sn.Value);
									break;
								case "collideconnected":
									collideConnected = bool.Parse(sn.Value);
									break;
								case "userdata":
									userData = ReadSimpleType(sn, null, false);
									break;
								}
							}

							mainDef.CollideConnected = collideConnected;
							mainDef.UserData = userData;
							_joints.Add(new JointDefSerialized(mainDef, bodyA, bodyB, name));
						}
					}
					break;
				}
			}

			return data;
		}
	}

	public class WorldSerializer : IWorldSerializationProvider
	{
		IWorldSerializer _serializer;

		List<ShapeSerialized> _shapeDefinitions = new List<ShapeSerialized>();
		List<FixtureDefSerialized> _fixtureDefinitions = new List<FixtureDefSerialized>();
		List<BodyDefSerialized> _bodyDefinitions = new List<BodyDefSerialized>();
		List<JointDefSerialized> _joints = new List<JointDefSerialized>();
		World _world;

		public World World
		{
			get { return _world; }
		}

		public IList<ShapeSerialized> Shapes
		{
			get { return _shapeDefinitions; }
		}

		public IList<FixtureDefSerialized> FixtureDefs
		{
			get { return _fixtureDefinitions; }
		}

		public IList<BodyDefSerialized> Bodies
		{
			get { return _bodyDefinitions; }
		}

		public IList<JointDefSerialized> Joints
		{
			get { return _joints; }
		}

		public int IndexOfFixture(FixtureDef def)
		{
			for (int i = 0; i < _fixtureDefinitions.Count; ++i)
				if (_fixtureDefinitions[i].Fixture == def)
					return i;

			return -1;
		}

		public int IndexOfBody(BodyDef def)
		{
			for (int i = 0; i < _bodyDefinitions.Count; ++i)
				if (_bodyDefinitions[i].Body == def)
					return i;

			return -1;
		}

		public WorldSerializer(IWorldSerializer serializer)
		{
			_serializer = serializer;
		}

		protected int FixtureIDFromFixture(FixtureDef def)
		{
			for (int i = 0; i < _fixtureDefinitions.Count; ++i)
			{
				if (_fixtureDefinitions[i].Fixture.CompareWith(def))
					return i;
			}

			throw new KeyNotFoundException();
		}

		public static WorldSerializer SerializeWorld(World world, IWorldSerializer serializer)
		{
			WorldSerializer worldSerializer = new WorldSerializer(serializer);
			worldSerializer._world = world;

			foreach (var body in world.Bodies)
			{
				BodyDef def = new BodyDef();

				def.Active = body.IsActive;
				def.AllowSleep = body.IsSleepingAllowed;
				def.Angle = body.Angle;
				def.AngularDamping = body.AngularDamping;
				def.AngularVelocity = body.AngularVelocity;
				def.Awake = body.IsAwake;
				def.BodyType = body.BodyType;
				def.Bullet = body.IsBullet;
				def.FixedRotation = body.IsFixedRotation;
				//def.InertiaScale
				def.LinearDamping = body.LinearDamping;
				def.LinearVelocity = body.LinearVelocity;
				def.Position = body.Position;
				def.UserData = body.UserData;

				List<FixtureDef> fixtures = new List<FixtureDef>();

				foreach (var f in body.Fixtures)
				{
					FixtureDef fixDef = new FixtureDef();
					fixDef.Density = f.Density;
					fixDef.Filter = f.FilterData;
					fixDef.Friction = f.Friction;
					fixDef.IsSensor = f.IsSensor;
					fixDef.Restitution = f.Restitution;
					fixDef.Shape = f.Shape;
					fixDef.UserData = f.UserData;

					fixtures.Add(fixDef);
				}

				worldSerializer.AddBody(body, def, fixtures);
			}

			return worldSerializer;
		}

		public void AddShape(Shape shape)
		{
			foreach (var s in _shapeDefinitions)
			{
				// shape already exists
				if (s.Shape.CompareWith(shape))
					return;
			}

			_shapeDefinitions.Add(new ShapeSerialized(shape, null));
		}

		public void AddFixture(FixtureDef fixture)
		{
			bool containsShape = false;
			int shapeID = -1;

			// see if we need to add the shape
			for (int i = 0; i < _shapeDefinitions.Count; ++i)
			{
				if (fixture.Shape == _shapeDefinitions[i].Shape)
				{
					containsShape = true;
					shapeID = i;
					break;
				}
			}

			// No, so let's add it
			if (!containsShape)
			{
				AddShape(fixture.Shape);
				shapeID = _shapeDefinitions.Count - 1;
			}

			_fixtureDefinitions.Add(new FixtureDefSerialized(fixture, shapeID, null));
		}

		public void AddBody(Body derivedBody, BodyDef body, List<FixtureDef> fixtures)
		{
			List<int> fixtureIDs = new List<int>();

			// See if we need to add any of the fixtures
			foreach (var f in fixtures)
			{
				try
				{
					fixtureIDs.Add(FixtureIDFromFixture(f));
				}
				catch (KeyNotFoundException)
				{
					AddFixture(f);
					fixtureIDs.Add(_fixtureDefinitions.Count - 1);
				}
			}

			_bodyDefinitions.Add(new BodyDefSerialized(derivedBody, body, fixtureIDs, null));
		}

		int IndexOfDerivedBody(Body b)
		{
			for (int i = 0; i < _bodyDefinitions.Count; ++i)
				if (_bodyDefinitions[i].DerivedBody == b)
					return i;

			return -1;
		}

		public void AddJoint(JointDef joint)
		{
			_joints.Add(new JointDefSerialized(joint, IndexOfDerivedBody(joint.BodyA), IndexOfDerivedBody(joint.BodyB), null));
		}

		public void Serialize(Stream stream)
		{
			_serializer.Open(stream, this);

			_serializer.BeginSerializingShapes();
			foreach (var s in _shapeDefinitions)
				_serializer.SerializeShape(s);
			_serializer.EndSerializingShapes();

			_serializer.BeginSerializingFixtures();
			foreach (var f in _fixtureDefinitions)
				_serializer.SerializeFixture(f);
			_serializer.EndSerializingFixtures();

			_serializer.BeginSerializingBodies();
			foreach (var b in _bodyDefinitions)
				_serializer.SerializeBody(b);
			_serializer.EndSerializingBodies();

			_serializer.BeginSerializingJoints();
			foreach (var j in _joints)
				_serializer.SerializeJoint(j);
			_serializer.EndSerializingJoints();

			_serializer.Close();
		}
	}
}
