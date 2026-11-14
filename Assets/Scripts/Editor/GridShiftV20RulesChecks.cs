using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class GridShiftV20RulesChecks {
 public static void Batch(){var report=new StringBuilder();int fails=0;Action<bool,string> check=(ok,name)=>{report.AppendLine((ok?"PASS ":"FAIL ")+name);if(!ok)fails++;};
  var root=new GameObject("Directed lane geometry fixture");var route=root.AddComponent<TrafficRoute>();var a=new GameObject("A");a.transform.SetParent(root.transform);var b=new GameObject("B");b.transform.SetParent(root.transform);b.transform.position=Vector3.forward*20;route.pontos=new[]{a.transform,b.transform};
  check(GridShiftRoadRules.WrongWay(Vector3.forward*10,Vector3.back,new[]{route}),"contramão detectada no centro de faixa dirigida");
  check(!GridShiftRoadRules.WrongWay(Vector3.forward*10,Vector3.forward,new[]{route}),"sentido correto não é contramão");
  check(!GridShiftRoadRules.WrongWay(new Vector3(5,0,10),Vector3.back,new[]{route}),"fora da faixa não recebe classificação por proximidade distante");
  check(!GridShiftRoadRules.WrongWay(new Vector3(0,4,10),Vector3.back,new[]{route}),"outro nível vertical não é classificado pela faixa abaixo");
  check(!GridShiftRoadRules.WrongWay(Vector3.forward*.2f,Vector3.back,new[]{route}),"extremidade de rota evita multa durante conversão");
  var second=new GameObject("Opposing ambiguous lane");var other=second.AddComponent<TrafficRoute>();other.pontos=new[]{b.transform,a.transform};check(!GridShiftRoadRules.WrongWay(Vector3.forward*10,Vector3.back,new[]{route,other}),"rotas opostas sobrepostas são ambíguas e não multam");
  check(GridShiftRoadRules.CrossedRed(Vector3.back,Vector3.forward,Vector3.zero,Vector3.forward,true),"cruza linha vermelha no sentido de entrada");check(!GridShiftRoadRules.CrossedRed(Vector3.forward,Vector3.back,Vector3.zero,Vector3.forward,true),"cruzamento reverso não é avanço do sinal dessa direção");check(!GridShiftRoadRules.CrossedRed(Vector3.back,Vector3.forward,Vector3.zero,Vector3.forward,false),"sinal verde não multa");check(!GridShiftRoadRules.CrossedRed(new Vector3(3,0,-1),new Vector3(3,0,1),Vector3.zero,Vector3.forward,true),"travessia fora da largura não multa");
  Object.DestroyImmediate(root);Object.DestroyImmediate(second);EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");var actual=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None);int realCases=0;foreach(var lane in actual){if(!lane.Valida)continue;for(int i=1;i<lane.pontos.Length;i++){var mid=(lane.pontos[i-1].position+lane.pontos[i].position)*.5f;var direction=Vector3.ProjectOnPlane(lane.pontos[i].position-lane.pontos[i-1].position,Vector3.up).normalized;if(lane.Cruzamento!=null&&Vector3.ProjectOnPlane(mid-lane.Cruzamento.transform.position,Vector3.up).magnitude<15)continue;if(GridShiftRoadRules.WrongWay(mid,-direction,actual)&&!GridShiftRoadRules.WrongWay(mid,direction,actual))realCases++;}}report.AppendLine("MEASURE realCityDirectedSegments="+realCases);check(realCases>0,"malha real da cidade detecta contramão e preserva sentido correto fora dos cruzamentos");report.AppendLine("RESULT "+(fails==0?"SUCCESS":"FAILURE"));Directory.CreateDirectory("Docs/Validacao20");File.WriteAllText("Docs/Validacao20/regras-geometria.txt",report.ToString());EditorApplication.Exit(fails==0?0:1);
 }
}

