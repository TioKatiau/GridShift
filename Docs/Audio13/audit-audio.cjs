const fs=require('fs'),path=require('path'),crypto=require('crypto');const manifest=require('./audio-manifest.json');let checks=0;const log=[];
for(const e of manifest){const b=fs.readFileSync(path.resolve(__dirname,'../..',e.path));let peak=0,energy=0;for(let i=44;i<b.length;i+=2){const v=b.readInt16LE(i)/32768;peak=Math.max(peak,Math.abs(v));energy+=v*v;}let seam=0;if(e.loop)for(let c=0;c<e.channels;c++)seam=Math.max(seam,Math.abs(b.readInt16LE(44+c*2)-b.readInt16LE(b.length-e.channels*2+c*2)));
 const valid=b.toString('ascii',0,4)==='RIFF'&&b.readUInt32LE(24)===48000&&peak>.001&&peak<.99&&(!e.loop||seam<=2)&&crypto.createHash('sha256').update(b).digest('hex')===e.sha256;
 log.push(`${valid?'PASS':'FAIL'} ${e.id}: ${e.seconds.toFixed(3)}s, ${e.channels}ch, peak=${peak.toFixed(4)}, rms=${Math.sqrt(energy/((b.length-44)/2)).toFixed(4)}, seam=${seam} PCM units`);if(!valid)throw Error(log.join('\n'));checks++;
}
log.push(`RESULT SUCCESS: ${checks} WAV files, 48 kHz, non-silent, no digital clipping, loop endpoints continuous. Numerical checks do not replace listening.`);fs.writeFileSync(path.join(__dirname,'asset-validation.txt'),log.join('\n'));console.log(log.at(-1));
