import {useEffect, useRef, useState} from 'react';
import {Check, Crosshair, Play} from 'lucide-react';
import {GunShape} from './GunHardware';
import {activateGunControl} from './GunNavigation';
import {t} from './i18n';
import type {Game, State} from './types';
import './launch-players.css';

export type PlayerSelection = {player:number; id:string}[];
export function LaunchPlayers({state, game, launch}:{state:State; game:Game; launch:(players:PlayerSelection)=>void}) {
  const guns=state.guns??[];
  const available=guns.filter(g=>g.driverHealthy&&g.mouseId);
  const [slots,setSlots]=useState<Record<number,string>>(()=>Object.fromEntries(state.bindings.map(b=>[b.player,guns.find(g=>g.id===b.physicalId||g.mouseId?.toLowerCase()===b.mouseId.toLowerCase())?.id??''])));
  const [active,setActive]=useState(1);
  const [last,setLast]=useState('');
  const lastShot=useRef<Record<string,number>>({});
  const single=state.playerSupport?.[game.id]?.maxPlayers===1;
  const select=(player:number,id:string)=>{
    setSlots(old=>({...old,[player]:id,...Object.fromEntries(Object.entries(old).filter(([p,g])=>+p!==player&&g===id).map(([p])=>[p,old[player]??'']))}));
    setActive(player===1&&!single?2:1);
  };
  useEffect(()=>{
    const shot=(event:Event)=>{
      const {id,x,y}=(event as CustomEvent<{id:string;x:number;y:number}>).detail;
      if(!available.some(g=>g.id===id))return;
      const now=performance.now();if(now-(lastShot.current[id]??-Infinity)<300)return;lastShot.current[id]=now;
      setLast(id);
      const hit=document.elementFromPoint(x*window.innerWidth,y*window.innerHeight);
      const slot=hit?.closest<HTMLElement>('[data-player-slot]');
      if(slot){if(!slot.hasAttribute('disabled'))select(Number(slot.dataset.playerSlot),id);return;}
      const control=hit?.closest<HTMLElement>('button,[role=button]');
      if(control&&!control.hasAttribute('disabled'))activateGunControl(control);
    };
    window.addEventListener('deadeye-gun-shot',shot);
    return()=>window.removeEventListener('deadeye-gun-shot',shot);
  },[available.map(g=>g.id).join('|'),single]);
  const selected=(p:number)=>available.find(g=>g.id===slots[p]);
  return <div className="launch-players">
    <h2>{t('Wer spielt?')}</h2>
    <p className="launch-game-title">{game.title}</p>
    <p className="launch-instruction">{t('Schieße mit deiner Gun auf P1 oder P2. Oder wähle einen Spieler und darunter seine Gun.')}</p>
    <div className="launch-slots">
      {[1,2].map(player=>{
        const gun=selected(player),disabled=single&&player===2;
        return <button key={player} data-player-slot={player} disabled={disabled} aria-pressed={active===player} className={'launch-player '+(gun?'assigned ':'')+(active===player?'choosing':'')} onClick={()=>setActive(player)}>
          <span className="launch-player-number">P{player}</span>
          {gun?<GunShape model={gun.systemId} mini/>:<Crosshair size={48} aria-hidden="true"/>}
          <strong>{disabled?t('Einzelspieler'):gun?.name??t('Hier anschießen')}</strong>
          <span>{gun?<><Check size={18}/>{t('Zugeordnet')}{gun.port?' · '+gun.port:''}</>:disabled?t('P2 wird in diesem Spiel nicht verwendet'):t('Mit der gewünschten Gun zielen und schießen')}</span>
        </button>;
      })}
    </div>
    {!single&&<button className="launch-sit-out" onClick={()=>{setSlots(old=>({...old,2:''}));setActive(1);}}>{t('Nur mit P1 spielen')}</button>}
    <div className="launch-guns-heading"><h3>{t('Verfügbare Guns')}</h3><span>{t('Auswahl für P{0}',active)}</span></div>
    <div className="launch-guns">
      {guns.map((gun,index)=>{
        const usable=available.some(g=>g.id===gun.id),player=[1,2].find(p=>slots[p]===gun.id);
        return <button key={gun.id} disabled={!usable} aria-pressed={!!player} className={'launch-gun '+(last===gun.id?'last-shot':'')} onClick={()=>select(active,gun.id)}>
          <GunShape model={gun.systemId} mini/>
          <span><strong>{gun.name}</strong><small>{gun.port??t('Gun {0}',index+1)} · {usable?t('Verbunden'):t('Nicht bereit')}</small></span>
          {player&&<b>P{player}</b>}
        </button>;
      })}
      {!guns.length&&<p>{t('Keine Gun erkannt. Du kannst trotzdem mit deiner gespeicherten Konfiguration starten.')}</p>}
    </div>
    <footer className="launch-player-actions">
      <p role="status">{available.length>0&&!selected(1)?t('P1 zuordnen oder mit der gespeicherten Konfiguration starten.'):t('Die Auswahl wird beim Start auf das Spielprofil übertragen.')}</p>
      <button className="primary launch-confirm" onClick={()=>launch(selected(1)?[1,2].filter(p=>selected(p)&&(!single||p===1)).map(player=>({player,id:slots[player]})):[])}><Play size={22}/>{selected(1)?t('Jetzt starten'):t('Mit gespeicherter Zuordnung starten')}</button>
    </footer>
  </div>;
}
