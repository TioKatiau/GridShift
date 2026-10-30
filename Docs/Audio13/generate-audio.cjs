// Original procedural compositions and sound design for GridShift. No external samples.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const root=path.resolve(__dirname,'../../Assets/Audio'),rate=48000;
let seed=13029;function random(){seed=(Math.imul(seed,1664525)+1013904223)>>>0;return seed/4294967296*2-1;}
const manifest=[];const tau=Math.PI*2;const midi=n=>440*2**((n-69)/12);
function save(folder,id,seconds,fn,loop=false,stereo=false){
 const n=Math.round(seconds*rate),ch=stereo?2:1,a=new Float32Array(n*ch);let peak=0,sum=0;
 for(let i=0;i<n;i++)for(let c=0;c<ch;c++){let v=fn(i/rate,i,c);if(!loop)v*=Math.min(1,i/240,(n-1-i)/480);a[i*ch+c]=v;}
 // Preserve exact musical duration and gently correct the final 40ms so the
 // loop endpoint meets the opening sample, without a one-sample forced jump.
 for(let c=0;c<ch;c++){let dc=0;for(let i=0;i<n;i++)dc+=a[i*ch+c]/n;for(let i=0;i<n;i++)a[i*ch+c]-=dc;
  if(loop){const k=1920,difference=a[(n-1)*ch+c]-a[c];for(let j=0;j<k;j++){let w=j/(k-1);a[(n-k+j)*ch+c]-=difference*w*w*(3-2*w);}}}
 for(let v of a)peak=Math.max(peak,Math.abs(v));let gain=peak>.88?.88/peak:1;
 const b=Buffer.alloc(44+n*ch*2);b.write('RIFF');b.writeUInt32LE(b.length-8,4);b.write('WAVEfmt ',8);b.writeUInt32LE(16,16);b.writeUInt16LE(1,20);b.writeUInt16LE(ch,22);b.writeUInt32LE(rate,24);b.writeUInt32LE(rate*ch*2,28);b.writeUInt16LE(ch*2,32);b.writeUInt16LE(16,34);b.write('data',36);b.writeUInt32LE(n*ch*2,40);
 for(let i=0;i<a.length;i++){const v=a[i]*gain;sum+=v*v;b.writeInt16LE(Math.round(Math.max(-1,Math.min(1,v))*32767),44+i*2);}
 const file=path.join(root,folder,id+'.wav');fs.mkdirSync(path.dirname(file),{recursive:true});fs.writeFileSync(file,b);
 manifest.push({id,path:path.relative(path.resolve(__dirname,'../..'),file).replaceAll('\\','/'),seconds:n/rate,channels:ch,loop,peak:peak*gain,rms:Math.sqrt(sum/a.length),sha256:crypto.createHash('sha256').update(b).digest('hex'),origin:'Original procedural synthesis; no external recordings or samples',license:'CC0-1.0 dedication for generated audio assets'});
}
function music(id,style){const bpm=style===1?80:90,beat=60/bpm,duration=beat*64,progression=[[45,52,57,60,64],[41,48,53,57,60],[48,55,60,64,67],[43,50,55,59,62]],motif=[76,0,72,74,0,67,71,0,72,0,67,64,0,67,71,74];
 save('Music',id,duration,(t,i,c)=>{const bar=Math.floor(t/(beat*4)),chord=progression[Math.floor(bar/4)%4],bt=t%beat,phase=t/(beat*4)%1;
 let v=0;for(let k=0;k<chord.length;k++){const f=midi(chord[k]);v+=.033*Math.sin(tau*f*t+c*.14*k)*(1+.2*Math.sin(tau*t/duration*4+k));v+=.009*Math.sin(tau*f*2*t+.3);}
 const step=Math.floor(t/(beat/2)),note=motif[step%16];if(note){let age=t%(beat/2);v+=(style===1?.045:.065)*Math.sin(tau*midi(note+(style===2?-12:0))*age)*Math.exp(-age*7)*Math.min(1,age/.01);}
 const bass=midi(chord[0]-12);v+=.09*Math.sin(tau*bass*t)*(.55+.45*Math.exp(-bt*5));
 if(style!==1){v+=.14*Math.sin(tau*(48*bt+7*(1-Math.exp(-bt*24))))*Math.exp(-bt*19);let half=t%(beat/2);v+=random()*.015*Math.exp(-half*65);if(Math.floor(t/beat)%4===2)v+=random()*.035*Math.exp(-bt*30);}
 return v*.78;},true,true);}
music('Menu_CityInMotion',0);music('Office_AfterHours',1);music('Mission_ForwardMotion',2);
for(const heavy of [false,true]){const id=heavy?'Truck':'Car',base=heavy?40:64;let low=0;
 save('Vehicles',id+'_Engine_Loop',4,(t)=>{low+=.08*(random()-low);let v=0;for(let k=1;k<10;k++)v+=Math.sin(tau*base*k*t+.1*Math.sin(tau*2*t))/(k*2.3);return .27*v*(.83+.17*Math.sin(tau*base/2*t))+.13*low;},true);
 save('Vehicles',id+'_Road_Loop',4,(t)=>{low+=.06*(random()-low);return low*.25+random()*.025;},true);
 save('Vehicles',id+'_Brake',.65,(t)=>random()*.12*Math.exp(-t*6)+.03*Math.sin(tau*(heavy?240:560)*t)*Math.exp(-t*8));
 save('Vehicles',id+'_Horn',.4,(t)=>.14*(Math.sin(tau*(heavy?155:350)*t)+.45*Math.sin(tau*(heavy?194:440)*t))*Math.min(1,t/.04)*Math.min(1,(.4-t)/.08));}
save('Vehicles','Truck_Reverse',.4,t=>.19*Math.sin(tau*880*t)*Math.min(1,t/.015)*Math.min(1,(.4-t)/.02));
for(const surface of ['Concrete','Asphalt','Grass','Office'])for(let variant=1;variant<=3;variant++){let low=0;save('Footsteps',`Footstep_${surface}_${variant}`, .22,t=>{low+=.18*(random()-low);let f=surface==='Office'?170:surface==='Grass'?80:110;return .24*Math.sin(tau*(f+variant*8)*t)*Math.exp(-t*45)+low*(surface==='Grass'?.5:.25)*Math.exp(-t*(surface==='Grass'?16:30));});}
save('Footsteps','Jump',.22,t=>random()*.09*Math.exp(-t*15)+.12*Math.sin(tau*(170*t-120*t*t))*Math.exp(-t*22));
save('Footsteps','Land',.35,t=>.32*Math.sin(tau*62*t)*Math.exp(-t*23)+random()*.15*Math.exp(-t*35));
for(const [id,kind] of [['City_Ambience_Day',0],['City_Ambience_Night',1],['Office_Ventilation',2],['Green_Wind',3]]){let low=0;save(kind===2?'Office':'Ambient',id,16,t=>{low+=.018*(random()-low);return low*(kind===2?.5:1.1)+.025*Math.sin(tau*(kind===2?100:40)*t)+.025*Math.sin(tau*61*t)*(.5+.5*Math.sin(tau*t/16));},true,true);}
save('Ambient','Bird_Chirp',.65,t=>.13*Math.sin(tau*(2200*t+100*Math.sin(t*24)))*Math.sin(Math.PI*t/.65)**2*(.5+.5*Math.sin(t*55)));
save('Ambient','Night_Insects',1.2,t=>.055*Math.sin(tau*3600*t)*(.5+.5*Math.sin(tau*16*t))*Math.sin(Math.PI*t/1.2)**2);
// Nonverbal, synthesized murmurs, not words or imitations of recorded people.
save('Ambient','Pedestrian_Murmur',1.4,t=>{let p=tau*(130*t+2*Math.sin(t*12));return .07*(Math.sin(p)+.3*Math.sin(p*3)+.18*Math.sin(p*7))*(.5+.5*Math.sin(t*19))*Math.sin(Math.PI*t/1.4)**2;});
const effects={UI:['UI_Hover','UI_Click','UI_Confirm','UI_Cancel','UI_Tab','UI_Error','Logo_GS','Notification'],Tablet:['Tablet_Open','Tablet_Close'],Office:['Computer_Open','Computer_Close'],Deliveries:['Delivery_Accepted','Vehicle_Assigned','Cargo_Collected','Delivery_Completed','Delivery_Late','Delivery_Cancelled'],Financial:['Money_Received','Bill_Paid','Employee_Hired','Upgrade_Purchased','Reputation_Up','Service_Completed'],Missions:['Mission_Completed','Act_Unlocked'],SaveSystem:['Save_Selected','Save_Completed','Save_Loaded','Save_Deleted']};
const signatures={Logo_GS:[45,57,64,69],Money_Received:[81,88],Upgrade_Purchased:[60,67,72,79],Mission_Completed:[57,64,69,76],Delivery_Completed:[64,69,73],Act_Unlocked:[52,59,64,71,76],Employee_Hired:[69,73,76],Reputation_Up:[76,79],Save_Completed:[84,88],Save_Loaded:[72,79,84],Delivery_Accepted:[69,76],Vehicle_Assigned:[67,74],Cargo_Collected:[64,71],UI_Click:[84],UI_Hover:[88],UI_Error:[52,48],UI_Tab:[79],Tablet_Open:[64,76],Computer_Open:[60,67,79]};
for(const [folder,ids] of Object.entries(effects))for(const id of ids){const success=/Completed|Received|Hired|Purchased|Unlocked|Logo/.test(id),negative=/Error|Cancel|Late|Close/.test(id),length=id==='Logo_GS'?1.6:id==='UI_Click'?.08:/Received|Reputation|Save_Completed/.test(id)?.45:success?.85:/Hover/.test(id)?.055:.23;
 const notes=signatures[id]||(success?[57,64,69,76]:negative?[64,60]:[76,81]);save(folder,id,length,t=>{let v=0;for(let j=0;j<notes.length;j++){let age=t-j*(success?.10:.035);if(age<0)continue;let f=midi(notes[j]);v+=.15*(Math.sin(tau*f*age)+.15*Math.sin(tau*f*2*age))*Math.exp(-age*(success?8:24))*Math.min(1,age/.006);}return v;});}
fs.mkdirSync(path.resolve(__dirname),{recursive:true});fs.writeFileSync(path.join(__dirname,'audio-manifest.json'),JSON.stringify(manifest,null,2));
console.log(`Generated ${manifest.length} original non-silent WAVs, ${(manifest.reduce((s,x)=>s+x.seconds,0)).toFixed(1)} seconds.`);
