using System;
using System.Collections.Generic;
using System.Threading;

namespace MRS {
	namespace Environment {

		public struct Predicate {
			public string name;
			public bool value;
		};


		//enum class WorldviewType
		//{
		//	Spatial1D,	// line world - operates with 1d line position
		//	Spatial2D,	// 2d world - operates on a 2d plane
		//	Spatial3D,	// 3D world	- operates in 3d space
		//	Reactive,	// pure sensor input defines action output, lookup table
		//	SymbolicLogic, // world defined by logic expressions
		//	ComplexWorld, // worldview composed of multiple different worldviews
		//	Internet,	// world for representing web agents worldview
		//	Topological // World represented by a graph

		//};

		public class Worldview
		{
		private Dictionary<string, Dictionary<string, Device.View>> all_views;
		private Dictionary<string, double> parameters;
		private Dictionary<string, bool> predicates;
		//private WorldviewType world_type;
        private string world_type;
		private Timer central_timer;
		private long time;
			//LinkedList<View> views;
			//std::vector<View*> attributes;
			//unsigned int attributeCount;
			//unsigned int fullView[5];
			//char* dataString;
			//char toDecimal(char);
			//View* frontView;
			//View* floorView;
			//DistanceView* obstacleView;
			//ViewPosition2D* relativePosition;
			//ViewPosition2D* absolutePosition;
			//ViewPosition2D* origin;
			///float distances[8];
			//float radValues[8];
			//float sinValues[8];
			//float cosValues[8];
			//float sinTotal, cosTotal;
		public Worldview(){}

		public void AddView(string key, Device.View view, string view_type){}
		public void RemoveView(){}
		public Device.View GetView(string view_key){
            return new Device.View();//"TODO implement";
        }
		public string GetViewType(string view_key){
            return "TODO implement";
        }
			
		public void SetWorldviewType(string world_type){}
		public string GetWorldviewType(){
            return "Spatial1D";
        }

        public bool GetPredicateValue(string name){
            return false; //TODO implement
        }
		public Predicate GetPredicate(string name){
            Predicate p;
            p.name = name;
            p.value = false;
            return p;
        }

		public double GetParameterValue(string name){
            return 0.0;
        }

		public Tuple<string, double> GetParameter(string name){
            return new Tuple<string, double>(name, 0.0);
        }

		public long GetTime(){ // Returns simulation time from timer
            return time;
        }

		public void Update(){

        }

		public virtual void OnUpdate(){

        }

			//bool hasAttribute(TypeDefinitions::ViewType);
			//char GetAttributePlace(TypeDefinitions::ViewType);
			//ViewPosition2D * GetPosition2D();
			//ViewText * GetTextString();
			//void ModifyAttribute(TypeDefinitions::ViewType, View*);
			//void Moved(float, float);
			//void Moved(int, int);
			//void Update();
			//void SetPositionView(View*);
			//void SetLocalPositionView(View*);
			//void SetOrigin(View*);
			//void SetSectorView(View*);
			//View* GetLocalPositionView();
			//View* GetOrigin();
			//View* GetPositionView();
			//View* GetSectorView();
			//void SetFloorView(View*);
			//View* GetFloorView();
			//char* GetFullViewString();
			//float GetAverageDistanceSin();
			//float GetAverageDistanceCos();
			// Some method templates
			//void GenerateDetectorOutput(char [][] detectorsOut, long timeStart, long timeEnd);
			//void GenerateGradientOutput(char [][] detectorsOut, long timeStart, long timeEnd);
			//void GenerateDistanceOutput(char [][] distancesOut, long timeStart, long timeEnd);
			//void GenerateTrajectory(char [][][] trajectoryOut, long timeStart, long timeEnd);
			//void GenerateObjectMap(char [][][][] mapOut, long timeStart,long timeEnd); // This generates map from trajectory + Distance output
			//void GenerateMarkerMap(char [][][][] mapOut, long timeStart,long timeEnd); // This generates map from trajectory + Detector output
		}
	}
}