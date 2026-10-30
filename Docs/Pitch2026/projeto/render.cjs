// Editable FFmpeg project. Titles, timing and ordering are in timeline.json.
const fs=require('fs'),path=require('path'),{spawnSync}=require('child_process');
const base=path.resolve(__dirname,'..'),root=path.resolve(base,'../..'),timeline=require('./timeline.json');
const ffmpeg=process.env.FFMPEG||'C:/Users/Gb191/AppData/Local/Temp/GridShiftPitchTools/ffmpeg/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe';
const fps=30,transition=timeline.transition,staging=path.join(base,'edicao');fs.mkdirSync(staging,{recursive:true});process.chdir(base);
function run(args,label){console.log(label);const result=spawnSync(ffmpeg,['-hide_banner','-loglevel','error','-nostats','-y',...args],{encoding:'utf8',maxBuffer:8e6});if(result.status!==0)throw Error(label+'\n'+result.stderr);}
function esc(s){return s.replaceAll('\\','/').replaceAll(':','\\:').replaceAll("'","\\'");}
const font='C\\:/Windows/Fonts/segoeui.ttf',bold='C\\:/Windows/Fonts/seguisb.ttf';
for(let i=0;i<timeline.segments.length;i++){
 const s=timeline.segments[i],duration=s.duration+(i<timeline.segments.length-1?transition:0),out=path.join(staging,s.id+'.mp4');
 if(process.argv.includes('--resume')&&fs.existsSync(out)&&fs.statSync(out).size>1000)continue;
 if(s.graphic){
  const last=s.graphic==='outro',frames=Math.round(duration*fps),v=`scale=1920:1080,zoompan=z='min(zoom+0.000035,1.015)':x='iw/2-iw/zoom/2':y='ih/2-ih/zoom/2':d=${frames}:s=1920x1080:fps=30,fade=t=in:st=0:d=${last?.65:1.05}${last?',fade=t=out:st=8.5:d=1.2':''},setsar=1`;
  run(['-loop','1','-i',`arte/${s.graphic}.png`,'-t',String(duration),'-vf',v,'-an','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p',out],s.id);continue;
 }
 const input=`clipes/${s.id}.mp4`;if(!fs.existsSync(input))throw Error('Missing real footage: '+input);
 fs.writeFileSync(path.join(staging,s.id+'-title.txt'),s.title);fs.writeFileSync(path.join(staging,s.id+'-kicker.txt'),s.kicker);
 const alpha="if(lt(t,0.4),0,if(lt(t,1),(t-0.4)/0.6,if(lt(t,"+(duration-.8)+"),1,max(0,("+(duration-.25)+"-t)/0.55))))";
 const titleSize=s.title.length>42?34:s.title.length>32?40:48;
 const y=s.ui?70:145,ky=s.ui?34:106;
 let grade=s.id.includes('noite')?'eq=contrast=1.02:saturation=0.91:gamma=1.18:brightness=0.008':'eq=contrast=1.025:saturation=0.94:brightness=-0.008';
 let filters=[s.ui?'scale=2016:1134:flags=lanczos,crop=1920:1080:48:27':'scale=1920:1080:flags=lanczos',`setsar=1`,`fps=30`,grade];
 if(!s.ui)filters.push('drawbox=x=0:y=0:w=iw:h=58:color=0x03070d:t=fill','drawbox=x=0:y=ih-58:w=iw:h=58:color=0x03070d:t=fill');
 filters.push(`drawbox=x=78:y=${ky}:w=4:h=${s.ui?79:94}:color=0xffcc25@0.9:t=fill`,`drawtext=fontfile='${bold}':textfile='edicao/${s.id}-kicker.txt':fontsize=17:fontcolor=0xffcc25:x=102:y=${ky}:alpha='${alpha}':shadowcolor=black@0.7:shadowx=1:shadowy=2`,`drawtext=fontfile='${bold}':textfile='edicao/${s.id}-title.txt':fontsize=${titleSize}:fontcolor=0xf4f7fb:x=102:y=${y}:alpha='${alpha}':shadowcolor=black@0.8:shadowx=2:shadowy=3`);
 if(s.ui)filters.splice(3,0,'drawbox=x=0:y=0:w=iw:h=138:color=0x030810@0.76:t=fill');
 run(['-i',input,'-t',String(duration),'-vf',filters.join(','),'-an','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p',out],s.id);
}
// Assemble in small groups, preserving exact starts at the requested scene boundaries.
let inputs=[],filter=[],elapsed=0,last='0:v';for(let i=0;i<timeline.segments.length;i++)inputs.push('-i',path.join(staging,timeline.segments[i].id+'.mp4'));
for(let i=1;i<timeline.segments.length;i++){elapsed+=timeline.segments[i-1].duration;let output='v'+i;filter.push(`[${last}][${i}:v]xfade=transition=fade:duration=${transition}:offset=${elapsed}[${output}]`);last=output;}
fs.writeFileSync(path.join(staging,'assemble-filter.txt'),filter.join(';\n'));
run([...inputs,'-filter_complex_threads','1','-filter_complex_script',path.join(staging,'assemble-filter.txt'),'-map',`[${last}]`,'-t','100','-an','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p','-movflags','+faststart',path.join(base,'GridShift_Pitch_Sem_Audio.mp4')],'Assemble silent 100-second master');
fs.mkdirSync(path.join(base,'audio'),{recursive:true});
const sources=path.join(base,'audio','fontes');fs.mkdirSync(sources,{recursive:true});for(const rel of ['Music/Menu_CityInMotion.wav','UI/Logo_GS.wav','UI/UI_Tab.wav']){let dest=path.join(sources,path.basename(rel));if(!fs.existsSync(dest))fs.copyFileSync(path.join(root,'Assets/Audio',rel),dest);}if(!fs.existsSync(path.join(sources,'LICENSE.txt')))fs.copyFileSync(path.join(root,'Assets/Audio/LICENSE.txt'),path.join(sources,'LICENSE.txt'));
const music=path.join(sources,'Menu_CityInMotion.wav'),logo=path.join(sources,'Logo_GS.wav'),whoosh=path.join(sources,'UI_Tab.wav');
let mix=['[0:a]atrim=0:100,asetpts=PTS-STARTPTS,volume=0.40,afade=t=in:d=2,afade=t=out:st=96:d=4[music]','[1:a]asplit=2[l1][l2]','[l1]volume=0.32,adelay=600|600[logo1]','[l2]volume=0.26,adelay=90600|90600[logo2]','[2:a]asplit=5[w1][w2][w3][w4][w5]'];
let pads=['music','logo1','logo2'];for(let j=0;j<5;j++){let n='s'+j;mix.push(`[w${j+1}]volume=0.22,adelay=${[12000,27000,47000,68000,87000][j]}|${[12000,27000,47000,68000,87000][j]}[${n}]`);pads.push(n);}mix.push(pads.map(n=>'['+n+']').join('')+`amix=inputs=${pads.length}:normalize=0:dropout_transition=0,alimiter=limit=0.85:level=0,atrim=0:100[out]`);
run(['-stream_loop','-1','-i',music,'-i',logo,'-i',whoosh,'-filter_complex',mix.join(';'),'-map','[out]','-t','100','-ar','48000','-ac','2','-c:a','pcm_s16le',path.join(base,'audio','Trilha_Discreta_Sem_Voz.wav')],'Original instrumental bed and gentle transitions, no voices');
run(['-i',path.join(base,'GridShift_Pitch_Sem_Audio.mp4'),'-i',path.join(base,'audio','Trilha_Discreta_Sem_Voz.wav'),'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','192k','-t','100','-movflags','+faststart',path.join(base,'GridShift_Pitch_Com_Trilha.mp4')],'MP4 with low instrumental bed');
console.log('Both final MP4 exports completed.');
