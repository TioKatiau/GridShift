# Biblioteca de áudio GridShift 13.0

Todos os 63 WAVs são síntese procedural original: sem amostras externas, música comercial ou gravações de terceiros. Licença dos WAVs: CC0-1.0; ver Assets/Audio/LICENSE.txt. Motores, ruído urbano e murmúrios são sintetizados, não gravações de campo.

## Music

- `Assets/Audio/Music/Menu_CityInMotion.wav` — 42.667 s; estéreo; loop.
- `Assets/Audio/Music/Office_AfterHours.wav` — 48.000 s; estéreo; loop.
- `Assets/Audio/Music/Mission_ForwardMotion.wav` — 42.667 s; estéreo; loop.

## Vehicles

- `Assets/Audio/Vehicles/Car_Engine_Loop.wav` — 4.000 s; mono; loop.
- `Assets/Audio/Vehicles/Car_Road_Loop.wav` — 4.000 s; mono; loop.
- `Assets/Audio/Vehicles/Car_Brake.wav` — 0.650 s; mono.
- `Assets/Audio/Vehicles/Car_Horn.wav` — 0.400 s; mono.
- `Assets/Audio/Vehicles/Truck_Engine_Loop.wav` — 4.000 s; mono; loop.
- `Assets/Audio/Vehicles/Truck_Road_Loop.wav` — 4.000 s; mono; loop.
- `Assets/Audio/Vehicles/Truck_Brake.wav` — 0.650 s; mono.
- `Assets/Audio/Vehicles/Truck_Horn.wav` — 0.400 s; mono.
- `Assets/Audio/Vehicles/Truck_Reverse.wav` — 0.400 s; mono.

## Footsteps

- `Assets/Audio/Footsteps/Footstep_Concrete_1.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Concrete_2.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Concrete_3.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Asphalt_1.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Asphalt_2.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Asphalt_3.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Grass_1.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Grass_2.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Grass_3.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Office_1.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Office_2.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Footstep_Office_3.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Jump.wav` — 0.220 s; mono.
- `Assets/Audio/Footsteps/Land.wav` — 0.350 s; mono.

## Ambient

- `Assets/Audio/Ambient/City_Ambience_Day.wav` — 16.000 s; estéreo; loop.
- `Assets/Audio/Ambient/City_Ambience_Night.wav` — 16.000 s; estéreo; loop.
- `Assets/Audio/Ambient/Green_Wind.wav` — 16.000 s; estéreo; loop.
- `Assets/Audio/Ambient/Bird_Chirp.wav` — 0.650 s; mono.
- `Assets/Audio/Ambient/Night_Insects.wav` — 1.200 s; mono.
- `Assets/Audio/Ambient/Pedestrian_Murmur.wav` — 1.400 s; mono.

## Office

- `Assets/Audio/Office/Office_Ventilation.wav` — 16.000 s; estéreo; loop.
- `Assets/Audio/Office/Computer_Open.wav` — 0.230 s; mono.
- `Assets/Audio/Office/Computer_Close.wav` — 0.230 s; mono.

## UI

- `Assets/Audio/UI/UI_Hover.wav` — 0.055 s; mono.
- `Assets/Audio/UI/UI_Click.wav` — 0.080 s; mono.
- `Assets/Audio/UI/UI_Confirm.wav` — 0.230 s; mono.
- `Assets/Audio/UI/UI_Cancel.wav` — 0.230 s; mono.
- `Assets/Audio/UI/UI_Tab.wav` — 0.230 s; mono.
- `Assets/Audio/UI/UI_Error.wav` — 0.230 s; mono.
- `Assets/Audio/UI/Logo_GS.wav` — 1.600 s; mono.
- `Assets/Audio/UI/Notification.wav` — 0.230 s; mono.

## Tablet

- `Assets/Audio/Tablet/Tablet_Open.wav` — 0.230 s; mono.
- `Assets/Audio/Tablet/Tablet_Close.wav` — 0.230 s; mono.

## Deliveries

- `Assets/Audio/Deliveries/Delivery_Accepted.wav` — 0.230 s; mono.
- `Assets/Audio/Deliveries/Vehicle_Assigned.wav` — 0.230 s; mono.
- `Assets/Audio/Deliveries/Cargo_Collected.wav` — 0.230 s; mono.
- `Assets/Audio/Deliveries/Delivery_Completed.wav` — 0.850 s; mono.
- `Assets/Audio/Deliveries/Delivery_Late.wav` — 0.230 s; mono.
- `Assets/Audio/Deliveries/Delivery_Cancelled.wav` — 0.230 s; mono.

## Financial

- `Assets/Audio/Financial/Money_Received.wav` — 0.450 s; mono.
- `Assets/Audio/Financial/Bill_Paid.wav` — 0.230 s; mono.
- `Assets/Audio/Financial/Employee_Hired.wav` — 0.850 s; mono.
- `Assets/Audio/Financial/Upgrade_Purchased.wav` — 0.850 s; mono.
- `Assets/Audio/Financial/Reputation_Up.wav` — 0.450 s; mono.
- `Assets/Audio/Financial/Service_Completed.wav` — 0.850 s; mono.

## Missions

- `Assets/Audio/Missions/Mission_Completed.wav` — 0.850 s; mono.
- `Assets/Audio/Missions/Act_Unlocked.wav` — 0.850 s; mono.

## SaveSystem

- `Assets/Audio/SaveSystem/Save_Selected.wav` — 0.230 s; mono.
- `Assets/Audio/SaveSystem/Save_Completed.wav` — 0.450 s; mono.
- `Assets/Audio/SaveSystem/Save_Loaded.wav` — 0.230 s; mono.
- `Assets/Audio/SaveSystem/Save_Deleted.wav` — 0.230 s; mono.

## Reproduzir a geração e a auditoria

Na raiz do projeto: `node Docs/Audio13/generate-audio.cjs`, seguido de `node Docs/Audio13/audit-audio.cjs`. O manifesto JSON registra duração, canais, origem, licença e hash SHA-256 de cada arquivo. Não execute integrate.cjs novamente: ele é o registro da aplicação inicial dos hooks.
