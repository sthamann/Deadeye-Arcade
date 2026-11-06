const {chromium}=require(require.resolve('playwright',{paths:[require('node:path').join(__dirname,'..','ui')]}));
const assert=require('node:assert/strict');
(async()=>{
 const browser=await chromium.launch();const page=await browser.newPage({viewport:{width:1440,height:1000}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{
  const listeners=[];
  const guns=['blamcon','blamcon','xgunner','rs3','sinden'].map((systemId,i)=>({id:'g'+i,name:systemId==='blamcon'?'Blamcon Vyper '+(i+1):'X-Gunner Wireless',systemId,mouseId:'m'+i,keyboardId:'k'+i,port:'COM'+(i+6),driverHealthy:true,inputIds:['m'+i,'k'+i],issues:[]}));
  const game={id:'aliens',title:'Aliens Armageddon',platform:'TeknoParrot',source:'teknoparrot',status:'ready',executable:'test.exe',arguments:[],workingDirectory:'C:/test',sourcePath:'test.xml',aspect:'16:9',favorite:false,cover:null,lastPlayed:null};
  const state={native:true,version:'test',games:[game],guns,bindings:guns.slice(0,2).map((g,i)=>({player:i+1,physicalId:g.id,mouseId:g.mouseId,keyboardId:g.keyboardId,systemId:g.systemId,serialPort:g.port})),devices:[],ports:[],installations:[],bindingStage:null,settings:{language:'en',fullscreen:true,startWithWindows:false,hasCoverKey:false},playerSupport:{aliens:{mode:'local-coop',maxPlayers:2,sources:[],checkedAt:''}}};
  window.fixture={state,sent:[],emit:(type,payload)=>listeners.forEach(fn=>fn({data:{type,payload}}))};
  window.chrome={webview:{addEventListener:(n,fn)=>listeners.push(fn),removeEventListener:()=>{},postMessage:m=>{window.fixture.sent.push(m);if(m.type==='ready')setTimeout(()=>window.fixture.emit('state',state),0);}}};
 });
 await page.goto(process.env.REAPER_PREVIEW_URL||'http://127.0.0.1:5199');
 await page.getByRole('button',{name:'Play',exact:true}).click();
 const play=page.getByRole('button',{name:'Launch game',exact:true});await play.evaluate(b=>b.click());
 await page.getByRole('heading',{name:'Who’s playing?'}).waitFor();
 assert.equal(await page.locator('.launch-gun').count(),5);
 const shot=async(selector,id)=>{
  const box=await page.locator(selector).boundingBox();assert(box);
  await page.evaluate(({x,y,id})=>{window.fixture.emit('gun-shot',{id,x:x/window.innerWidth,y:y/window.innerHeight});window.fixture.emit('input',{deviceId:'m'+id.slice(1),kind:'mouse',player:0,x:x/window.innerWidth,y:y/window.innerHeight,buttons:1});},{x:box.x+box.width/2,y:box.y+box.height/2,id});
 };
 await shot('[data-player-slot="1"]','g2');
 assert.match(await page.locator('[data-player-slot="1"]').innerText(),/X-Gunner/);
 assert.equal(await page.locator('[data-player-slot="2"] .launch-player-number').innerText(),'P2');
 // A second physical gun and the duplicate legacy/raw click cannot undo a choice.
 await page.waitForTimeout(320);await shot('[data-player-slot="2"]','g0');
 assert.match(await page.locator('[data-player-slot="2"]').innerText(),/Vyper 1/);
 await page.setViewportSize({width:1600,height:800});
 const confirm=await page.locator('.launch-confirm').boundingBox();assert(confirm&&confirm.y>=0&&confirm.y+confirm.height<=800,'Launch remains visible with five guns');
 await page.screenshot({path:'work/start-mapping-20261106/player-selection-wide.png'});
 await page.setViewportSize({width:600,height:900});
 assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
 await page.screenshot({path:'work/start-mapping-20261106/player-selection-small.png'});
 await page.locator('.launch-confirm').evaluate(b=>b.click());
 const launch=await page.evaluate(()=>window.fixture.sent.find(m=>m.type==='launch'));
 assert.deepEqual(launch.payload.players,[{player:1,id:'g2'},{player:2,id:'g0'}]);
 await page.evaluate(()=>{const s=window.fixture.state;s.settings.language='de';s.playerSupport.aliens.maxPlayers=1;window.fixture.emit('state',s)});
 await page.getByRole('button',{name:'Spiel starten',exact:true}).evaluate(b=>b.click());
 await page.getByRole('heading',{name:'Wer spielt?'}).waitFor();
 assert.equal(await page.locator('[data-player-slot="2"]').isDisabled(),true,'Single player title leaves P2 unused');
 await page.locator('.modal-close').evaluate(b=>b.click());
 await page.evaluate(()=>{const s=window.fixture.state;s.guns=[];s.bindings=[];window.fixture.emit('state',s)});
 await page.getByRole('button',{name:'Spiel starten',exact:true}).evaluate(b=>b.click());
 await page.getByRole('button',{name:'Mit gespeicherter Zuordnung starten',exact:true}).evaluate(b=>b.click());
 const saved=await page.evaluate(()=>window.fixture.sent.filter(m=>m.type==='launch').at(-1));assert.deepEqual(saved.payload.players,[],'No gun/test completion requirement blocks saved setup');
 assert.deepEqual(errors,[]);console.log('PASS: unassigned gun joins P1, separate gun joins P2, duplicate input is ignored, launch receives both physical identities, layouts fit 1440 and 600px');
 await browser.close();
})().catch(e=>{console.error(e);process.exitCode=1});
