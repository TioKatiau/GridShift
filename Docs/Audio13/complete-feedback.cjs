const fs=require('fs');function edit(name,changes){const file='Assets/Scripts/'+name;let s=fs.readFileSync(file,'utf8');for(const [a,b]of changes){if(!s.includes(a))throw Error(name+': missing '+a);s=s.replace(a,b);}fs.writeFileSync(file,s);}
edit('GridShiftSavesUI.cs',[
 ['if(!GridShiftProfiles.Create(slot,name,confirmed,out var error)){status.text=error;return;}','if(!GridShiftProfiles.Create(slot,name,confirmed,out var error)){GridShiftAudio.Play("UI_Error","UI",.3f);status.text=error;return;}'],
 ['else status.text="Não foi possível salvar. Aguarde o fim da transição.";','else{GridShiftAudio.Play("UI_Error","UI",.3f);status.text="Não foi possível salvar. Aguarde o fim da transição.";}'],
 ['if(!GridShiftProfiles.Read(slot,out var data,out _)){status.text="Este slot está vazio ou inválido.";','if(!GridShiftProfiles.Read(slot,out var data,out _)){GridShiftAudio.Play("UI_Error","UI",.3f);status.text="Este slot está vazio ou inválido.";'],
 ['GridShiftUI.Button(content,"Confirmar",()=>action(),55,true);','GridShiftUI.Button(content,"Confirmar",()=>{GridShiftAudio.Play("UI_Confirm","UI",.3f);action();},55,true);'],
 ['GridShiftProfiles.Delete(slot,true,out _);Render();','if(GridShiftProfiles.Delete(slot,true,out _))GridShiftAudio.Play("Save_Deleted","UI",.3f);else GridShiftAudio.Play("UI_Error","UI",.3f);Render();'],
 ['else status.text=error;}));}else GridShiftUI.Button','else{GridShiftAudio.Play("UI_Error","UI",.3f);status.text=error;}}));}else GridShiftUI.Button']
]);
edit('GridShiftCompany.cs',[
 ['else h.fuel=100;NotifyChanged();Save();return true;','else{h.fuel=100;GridShiftAudio.Play("Service_Completed","SFX",.3f);}NotifyChanged();Save();return true;'],
 ['h.maintenanceUntil=0;h.condition=100;Manager.Notify','h.maintenanceUntil=0;h.condition=100;GridShiftAudio.Play("Service_Completed","SFX",.35f,2);Manager.Notify']
]);
edit('GridShiftFinance.cs',[[ 'reminder.reminded=true;manager.Notify','reminder.reminded=true;GridShiftAudio.Play("Notification","SFX",.25f,2);manager.Notify' ]]);
edit('GridShiftMissions.cs',[[ 'S.eventExpiry=S.nextEventDay;var low=','S.eventExpiry=S.nextEventDay;GridShiftAudio.Play("Notification","SFX",.25f,2);var low=' ]]);
console.log('Save validation, confirmations, service completion and real new-demand events connected.');
