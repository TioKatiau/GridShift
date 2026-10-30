const fs=require('fs');
const {chromium}=require('C:/Users/Gb191/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
async function run(file){
 const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files/BraveSoftware/Brave-Browser/Application/brave.exe'});
 const page=await browser.newPage({viewport:{width:1200,height:800}});
 const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.setContent(fs.readFileSync(file,'utf8'));
 const results=await page.evaluate(async()=>{
  window.uwb={ExecuteJsMethod(){}};window.gridshiftSession({user:'Teste'});
  const data={money:100,selectedId:'order-01',concurrentLimit:3,points:[{id:'a',name:'Loja A',type:'Comércio',accessible:true,unlocked:true,x:0,z:0},{id:'b',name:'Loja B',type:'Comércio',accessible:true,unlocked:true,x:100,z:100}],fleet:[{id:'GS-01',kind:'Truck',state:'Transporting',capacityKg:2000,speed:3,deliveryId:'order-01',x:25,z:25}],orders:[{id:'order-01',goods:'Carga',weightKg:500,vehicleId:'GS-01',status:3,pickupId:'a',destinationId:'b',estimatedMeters:100,reward:50,travelledMeters:25}],offers:Array.from({length:100},(_,i)=>({id:'offer-'+i,goods:'Contrato '+i,kind:0,weightKg:10,pickupId:'a',destinationId:'b'})),upgrades:[{id:'speed',category:'Frota',level:0,maxLevel:3,name:'Motor',description:'Melhorar',cost:50,canBuy:true}]};
  const tick=()=>new Promise(r=>requestAnimationFrame(()=>requestAnimationFrame(r)));
  window.gridshiftState(data);await tick();
  const first=document.querySelector('#orders article');let mutations=0;const observer=new MutationObserver(m=>mutations+=m.length);observer.observe(document.querySelector('#orders'),{childList:true,subtree:true});
  const times=[];
  for(let i=0;i<30;i++){const at=performance.now();data.orders[0].travelledMeters++;window.gridshiftState(structuredClone(data));await tick();times.push(performance.now()-at);}
  const stable=document.querySelector('#orders article')===first;
  document.querySelector('button[data-tab=available]').click();await tick();
  const offer=document.querySelector('#offers button');offer.focus();window.scrollTo(0,500);const scrollY=window.scrollY;
  window.gridshiftState(structuredClone(data));await tick();
  const focusStable=document.activeElement===offer,scrollStable=window.scrollY===scrollY;
  document.querySelector('button[data-tab=upgrades]').click();await tick();const upgrades=document.querySelectorAll('#upgrade-list article').length;
  document.querySelector('button[data-tab=history]').click();await tick();
  observer.disconnect();return {stable,mutations,focusStable,scrollStable,offers:document.querySelectorAll('#offers article').length,upgrades,telemetryToPaintMeanMs:times.reduce((a,b)=>a+b,0)/times.length};
 });await browser.close();return {file,...results,errors};
}
(async()=>{const after=await run('Assets/StreamingAssets/GridShiftDemo/index.html');const before=await run('C:/Users/Gb191/.codex/backups/GridShift-v8-before-20261025/StreamingAssets/GridShiftDemo/index.html');const result={before,after,success:after.stable&&after.mutations===0&&after.focusStable&&after.scrollStable&&after.offers===100&&after.upgrades===1&&after.errors.length===0};fs.writeFileSync('Docs/Validacao8/demo-performance.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));if(!result.success)process.exitCode=1;})().catch(e=>{console.error(e);process.exitCode=1});
