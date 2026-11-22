(() => {
  const $ = (s, root=document) => root.querySelector(s);
  const $$ = (s, root=document) => [...root.querySelectorAll(s)];
  const folders = [
    {id:1,name:'项目资料',alias:'Project Docs',desc:'产品需求、设计文档与项目管理资料',path:'D:\\Workspace\\Projects',cats:['项目','工作'],icon:'📁',tone:'#eaf2ff',pinned:false},
    {id:2,name:'开发代码',alias:'Source Code',desc:'日常开发仓库与本地代码工作区',path:'D:\\Workspace\\Code',cats:['项目','工作'],icon:'💻',tone:'#e8f4ef',pinned:true},
    {id:3,name:'设计资源',alias:'Design Assets',desc:'界面素材、图标、参考图与设计稿',path:'D:\\Workspace\\Design',cats:['项目'],icon:'🎨',tone:'#f5edff',pinned:false},
    {id:4,name:'工作文档',alias:'Work',desc:'日常办公文件、会议记录和工作资料',path:'D:\\Documents\\Work',cats:['工作'],icon:'🗂️',tone:'#fff3e6',pinned:false},
    {id:5,name:'个人照片',alias:'Photos',desc:'生活照片与个人影像归档',path:'E:\\Pictures',cats:['个人'],icon:'🖼️',tone:'#fff0f0',pinned:false},
    {id:6,name:'学习资料',alias:'Learning',desc:'技术学习、课程与阅读资料',path:'D:\\Documents\\Learning',cats:['个人','工作'],icon:'📚',tone:'#edf4ff',pinned:false},
    {id:7,name:'临时交换',alias:'Exchange',desc:'跨设备交换与临时中转文件',path:'E:\\Exchange',cats:['未分类'],icon:'📦',tone:'#eef2f0',pinned:false},
    {id:8,name:'归档备份',alias:'Archive',desc:'历史项目与长期保留的归档内容',path:'F:\\Archive',cats:['个人'],icon:'🗄️',tone:'#f2f0e9',pinned:false}
  ];
  let categories = ['全部文件夹','工作','项目','个人','未分类'];
  let activeCategory = '全部文件夹';
  let selectedId = null, nextId = 9, mainLocked = false, keepCards = true, opacity = 88, listMode = false;
  const THEME_KEY = 'folderhub-demo-theme';
  const currentTheme = () => document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
  function syncThemeUi(){
    const dark = currentTheme() === 'dark';
    const btn = $('#theme-btn');
    const chip = $('#theme-chip');
    if(btn){btn.textContent = dark ? '☀' : '☾';btn.title = dark ? '切换到浅色主题' : '切换到深色主题';btn.setAttribute('aria-label',btn.title);}
    if(chip){
      $('.theme-chip-icon',chip).textContent = dark ? '☀' : '☾';
      $('.theme-chip-label',chip).textContent = dark ? '浅色' : '深色';
      chip.title = dark ? '切换到浅色主题' : '切换到深色主题';
      chip.setAttribute('aria-label',chip.title);
    }
    const lightBtn = $('#theme-light');
    const darkBtn = $('#theme-dark');
    if(lightBtn) lightBtn.classList.toggle('active', !dark);
    if(darkBtn) darkBtn.classList.toggle('active', dark);
  }
  function applyWindowOpacity(){
    const rgb = getComputedStyle(document.documentElement).getPropertyValue('--panel-rgb').trim() || '255,255,255';
    $('#app-window').style.background = `rgba(${rgb},${opacity/100})`;
  }
  function applyTheme(theme, {silent=false}={}){
    const next = theme === 'dark' ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', next);
    try{localStorage.setItem(THEME_KEY, next);}catch(e){}
    syncThemeUi();
    applyWindowOpacity();
    if(!silent) toast(next === 'dark' ? '已切换到深色主题' : '已切换到浅色主题');
  }
  function toggleTheme(){applyTheme(currentTheme() === 'dark' ? 'light' : 'dark');}
  const catIcons = {'全部文件夹':'▦','工作':'▣','项目':'◈','个人':'♡','未分类':'□'};
  const categoryList = $('#category-list'), grid = $('#folder-grid'), widgets = $('#desktop-widgets');
  const categoryCount = name => name==='全部文件夹' ? folders.length : folders.filter(f=>f.cats.includes(name)).length;
  const shownFolders = () => {
    const q = $('#search-input').value.trim().toLocaleLowerCase();
    return folders.filter(f => (q || activeCategory==='全部文件夹' || f.cats.includes(activeCategory)) && (!q || f.name.toLocaleLowerCase().includes(q)||f.alias.toLocaleLowerCase().includes(q)));
  };
  function renderCategories(){
    categoryList.innerHTML = categories.map(c=>`<button class="category-item ${c===activeCategory?'active':''}" data-category="${esc(c)}"><span class="cat-icon">${catIcons[c]||'◇'}</span><span class="cat-label">${esc(c)}</span><span class="cat-count">${categoryCount(c)}</span></button>`).join('');
  }
  function renderFolders(){
    const items=shownFolders();
    $('#result-count').textContent=`${items.length} 个项目`;
    $('#folder-count').textContent=folders.length;
    grid.classList.toggle('list-mode',listMode);
    grid.innerHTML=items.map(f=>`<article class="folder-card ${selectedId===f.id?'selected':''}" data-id="${f.id}" tabindex="0" aria-label="${esc(f.name)}">
      <div class="folder-top"><div class="folder-icon" style="background:${f.tone}">${f.icon}</div><button class="folder-more" data-more="${f.id}" title="更多操作">···</button></div>
      <div class="folder-name">${esc(f.alias||f.name)}</div><div class="folder-alias">${f.alias?esc(f.name):'文件夹'} · ${esc(f.cats.join(' / '))}</div><p class="folder-desc">${esc(f.desc||'暂无用途说明')}</p>
      <div class="folder-bottom"><span class="folder-path" title="${esc(f.path)}">${esc(f.path)}</span><button class="pin-toggle ${f.pinned?'pinned':''}" data-pin="${f.id}">${f.pinned?'✓ 已固定':'＋ 固定到桌面'}</button></div></article>`).join('');
    $('#empty-state').classList.toggle('hidden',items.length!==0);
    grid.style.display=items.length?'grid':'none';
    renderCategories();renderWidgets();
  }
  function renderWidgets(){
    widgets.innerHTML=folders.filter(f=>f.pinned).map((f,i)=>`<article class="desktop-widget ${f.locked?'locked':''}" data-widget="${f.id}" style="left:${24+(i%3)*205}px;top:${125+Math.floor(i/3)*140}px"><div class="widget-head"><span class="widget-icon" style="background:${f.tone}">${f.icon}</span><div class="widget-controls"><button data-widget-lock="${f.id}" title="${f.locked?'解锁':'锁定'}">${f.locked?'🔒':'⌑'}</button><button data-widget-close="${f.id}" title="取消固定">×</button></div></div><div class="widget-name">${esc(f.alias||f.name)}</div><div class="widget-desc">${esc(f.desc||'')}</div><div class="widget-hint">${f.locked?'位置已锁定':'拖动卡片可调整位置'}</div></article>`).join('');
    enableWidgetDrag();
  }
  function esc(s){return String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));}
  function toast(message,type=''){const el=document.createElement('div');el.className=`toast ${type}`;el.textContent=message;$('#toast-region').append(el);setTimeout(()=>el.remove(),2600);}
  function openFolder(id){const f=folders.find(x=>x.id===id);if(!f)return;toast(`演示操作：将通过资源管理器打开「${f.name}」\n${f.path}`);}
  function showModal(html){$('#modal').innerHTML=html;$('#modal-backdrop').classList.remove('hidden');}
  function closeModal(){$('#modal-backdrop').classList.add('hidden');}
  function folderForm(f){
    const editing=!!f;
    showModal(`<div class="modal-header"><div><h2 id="modal-title">${editing?'编辑文件夹':'添加文件夹'}</h2><p>维护文件夹记录、别名和用途说明</p></div><button class="modal-close" data-close>×</button></div>
      <form id="folder-form"><div class="form-field"><label>文件夹名称 *</label><input name="name" required value="${esc(f?.name||'')}" placeholder="例如：项目资料"></div><div class="form-field"><label>别名（可选）</label><input name="alias" value="${esc(f?.alias||'')}" placeholder="例如：Project Docs"></div><div class="form-field"><label>真实路径 *</label><input name="path" required value="${esc(f?.path||'')}" placeholder="例如：D:\\Workspace\\Projects"></div><div class="form-field"><label>用途说明</label><textarea name="desc" placeholder="简要说明这个文件夹的用途">${esc(f?.desc||'')}</textarea></div><div class="form-field"><label>所属分类</label><select name="category" multiple size="${Math.min(4,categories.filter(c=>c!=='全部文件夹').length)}">${categories.filter(c=>c!=='全部文件夹').map(c=>`<option ${f?.cats.includes(c)?'selected':''}>${esc(c)}</option>`).join('')}</select><small class="form-hint">按住 Ctrl 可选择多个分类；分类为平级关系</small></div><div class="form-actions"><button type="button" class="secondary-button" data-close>取消</button><button class="modal-primary" type="submit">保存记录</button></div></form>`);
    $('#folder-form').addEventListener('submit',e=>{e.preventDefault();const d=new FormData(e.currentTarget);const path=d.get('path').trim();const duplicate=folders.find(x=>x.path.toLowerCase()===path.toLowerCase()&&x.id!==f?.id);if(duplicate){toast('该路径已存在，演示中不创建重复记录','error');return;}const cats=$('select[name="category"]',e.currentTarget).selectedOptions.length?[...$('select[name="category"]',e.currentTarget).selectedOptions].map(o=>o.value):['未分类'];const values={name:d.get('name').trim(),alias:d.get('alias').trim(),path,desc:d.get('desc').trim(),cats};if(editing)Object.assign(f,values);else folders.push({id:nextId++,...values,icon:'📁',tone:'#edf3ff',pinned:false});closeModal();renderFolders();toast(editing?'文件夹记录已更新':'文件夹记录已添加');});
  }
  function settingsModal(){showModal(`<div class="modal-header"><div><h2 id="modal-title">设置</h2><p>FolderHub 桌面组件与运行偏好</p></div><button class="modal-close" data-close>×</button></div>
    <div class="settings-section"><div class="settings-row"><div><strong>界面主题</strong><small>浅色 / 深色可随时切换；选择会保存在本机浏览器</small></div><div class="theme-toggle-group" role="group" aria-label="界面主题"><button type="button" class="theme-toggle-btn" id="theme-light">浅色</button><button type="button" class="theme-toggle-btn" id="theme-dark">深色</button></div></div></div>
    <div class="settings-section"><div class="settings-row"><div><strong>主组件固定到桌面</strong><small>开启后主组件显示在桌面壁纸层；演示中用于模拟固定状态</small></div><input type="checkbox" id="main-pin"></div></div>
    <div class="settings-section"><div class="settings-row"><div><strong>锁定主组件位置</strong><small>锁定后不可拖动或调整位置</small></div><input type="checkbox" id="main-lock" ${mainLocked?'checked':''}></div></div>
    <div class="settings-section"><div class="settings-row"><div><strong>退出主程序后保留独立卡片</strong><small>全局统一开关，默认开启；不提供逐卡片覆盖</small></div><input type="checkbox" id="keep-cards" ${keepCards?'checked':''}></div></div>
    <div class="settings-section"><div class="settings-row"><div><strong>背景透明度</strong><small>仅调整主组件背景，文字和图标保持不透明</small></div><span class="range-value" id="opacity-value">${opacity}%</span></div><div class="range-row"><input id="opacity-range" type="range" min="20" max="100" value="${opacity}"></div></div>
    <div class="form-actions"><button class="modal-primary" data-close>完成</button></div>`);
    syncThemeUi();
    $('#theme-light').addEventListener('click',()=>applyTheme('light'));
    $('#theme-dark').addEventListener('click',()=>applyTheme('dark'));
    $('#main-pin').addEventListener('change',e=>{toast(e.target.checked?'主组件固定到桌面：开启（演示）':'主组件固定到桌面：关闭（演示）');});
    $('#main-lock').addEventListener('change',e=>{mainLocked=e.target.checked;toast(mainLocked?'主组件位置已锁定':'主组件位置已解锁');});
    $('#keep-cards').addEventListener('change',e=>{keepCards=e.target.checked;toast(keepCards?'退出后保留独立卡片：开启':'退出后保留独立卡片：关闭');});
    $('#opacity-range').addEventListener('input',e=>{opacity=Number(e.target.value);$('#opacity-value').textContent=opacity+'%';applyWindowOpacity();});
  }
  function contextMenu(id,x,y){selectedId=id;const f=folders.find(v=>v.id===id);const m=$('#context-menu');m.innerHTML=`<button data-action="open" data-id="${id}">↗　打开文件夹</button><button data-action="edit" data-id="${id}">✎　编辑记录</button><button data-action="pin" data-id="${id}">${f.pinned?'⊖　取消固定':'⊕　固定到桌面'}</button><button class="danger" data-action="delete" data-id="${id}">×　删除记录</button>`;m.style.left=Math.min(x,innerWidth-175)+'px';m.style.top=Math.min(y,innerHeight-160)+'px';m.classList.remove('hidden');}
  function hideMain(){ $('#app-window').classList.add('hidden-window');$('#tray-popover').classList.add('hidden');toast('主组件已隐藏；可从托盘图标重新打开'); }
  function enableWidgetDrag(){
    $$('.desktop-widget',widgets).forEach(el=>{let startX,startY,left,top,moved=false;el.addEventListener('pointerdown',e=>{if(e.target.closest('button')||el.classList.contains('locked'))return;startX=e.clientX;startY=e.clientY;left=el.offsetLeft;top=el.offsetTop;el.classList.add('dragging');el.setPointerCapture(e.pointerId);});el.addEventListener('pointermove',e=>{if(!el.classList.contains('dragging'))return;moved=true;el.style.left=Math.max(0,Math.min(innerWidth-210,left+e.clientX-startX))+'px';el.style.top=Math.max(0,Math.min(innerHeight-180,top+e.clientY-startY))+'px';});el.addEventListener('pointerup',()=>{el.classList.remove('dragging');if(moved)toast('卡片位置已调整（仅本次演示会话）');moved=false;});});
  }
  categoryList.addEventListener('click',e=>{const b=e.target.closest('[data-category]');if(!b)return;activeCategory=b.dataset.category;$('#current-category-title').textContent=activeCategory;$('#current-category-subtitle').textContent=activeCategory==='全部文件夹'?'所有常用位置，一处快速访问':`${activeCategory} 分类中的文件夹`;renderFolders();});
  grid.addEventListener('click',e=>{const pin=e.target.closest('[data-pin]');if(pin){const f=folders.find(x=>x.id===Number(pin.dataset.pin));f.pinned=!f.pinned;renderFolders();toast(f.pinned?'已创建独立桌面卡片':'已取消独立桌面卡片');return;}const more=e.target.closest('[data-more]');if(more){const r=more.getBoundingClientRect();contextMenu(Number(more.dataset.more),r.right,r.bottom);return;}const card=e.target.closest('[data-id]');if(card){selectedId=Number(card.dataset.id);renderFolders();if(e.detail===2)openFolder(selectedId);}});
  grid.addEventListener('contextmenu',e=>{const card=e.target.closest('[data-id]');if(card){e.preventDefault();contextMenu(Number(card.dataset.id),e.clientX,e.clientY);}});
  $('#search-input').addEventListener('input',renderFolders);
  $('#add-folder-btn').addEventListener('click',()=>folderForm(null));
  $('#add-category-btn').addEventListener('click',()=>showModal(`<div class="modal-header"><div><h2 id="modal-title">新增分类</h2><p>分类为平级列表，不支持嵌套</p></div><button class="modal-close" data-close>×</button></div><form id="category-form"><div class="form-field"><label>分类名称 *</label><input name="name" required maxlength="20" placeholder="输入分类名称"></div><div class="form-actions"><button type="button" class="secondary-button" data-close>取消</button><button class="modal-primary">创建分类</button></div></form>`));
  $('#modal').addEventListener('submit',e=>{if(e.target.id!=='category-form')return;e.preventDefault();const name=new FormData(e.target).get('name').trim();if(!name||categories.includes(name)||name==='全部文件夹'){toast('分类名称为空或已存在','error');return;}categories.push(name);closeModal();renderFolders();toast(`已创建分类「${name}」`);});
  $('#theme-btn').addEventListener('click',e=>{e.stopPropagation();toggleTheme();});
  $('#theme-chip').addEventListener('click',toggleTheme);
  $('#settings-btn').addEventListener('click',settingsModal);$('#hide-btn').addEventListener('click',hideMain);$('#minimize-btn').addEventListener('click',hideMain);
  const appWindow=$('#app-window'), dragHandle=$('#window-drag-handle');let windowDrag=null;dragHandle.addEventListener('pointerdown',e=>{if(mainLocked||e.target.closest('button'))return;const r=appWindow.getBoundingClientRect();windowDrag={x:e.clientX,y:e.clientY,left:r.left,top:r.top};appWindow.style.transform='none';appWindow.style.left=r.left+'px';appWindow.style.top=r.top+'px';dragHandle.setPointerCapture(e.pointerId);});dragHandle.addEventListener('pointermove',e=>{if(!windowDrag)return;appWindow.style.left=Math.max(0,Math.min(innerWidth-appWindow.offsetWidth,windowDrag.left+e.clientX-windowDrag.x))+'px';appWindow.style.top=Math.max(0,Math.min(innerHeight-appWindow.offsetHeight-44,windowDrag.top+e.clientY-windowDrag.y))+'px';});dragHandle.addEventListener('pointerup',()=>{if(windowDrag)toast('主组件位置已调整（仅本次演示会话）');windowDrag=null;});
  $('#tray-button').addEventListener('click',()=>$('#tray-popover').classList.toggle('hidden'));
  $('#tray-popover').addEventListener('click',e=>{const b=e.target.closest('[data-tray]');if(!b)return;$('#tray-popover').classList.add('hidden');if(b.dataset.tray==='show')$('#app-window').classList.remove('hidden-window');if(b.dataset.tray==='settings')settingsModal();if(b.dataset.tray==='exit'){if(keepCards)toast('演示：主程序退出，独立卡片按全局设置继续显示');else{folders.forEach(f=>f.pinned=false);renderFolders();toast('演示：主程序退出，独立卡片已全部关闭');}hideMain();}});
  widgets.addEventListener('click',e=>{const lock=e.target.closest('[data-widget-lock]');if(lock){const f=folders.find(x=>x.id===Number(lock.dataset.widgetLock));f.locked=!f.locked;renderWidgets();toast(f.locked?'卡片位置已锁定':'卡片位置已解锁');}const close=e.target.closest('[data-widget-close]');if(close){const f=folders.find(x=>x.id===Number(close.dataset.widgetClose));f.pinned=false;renderFolders();toast('已取消固定');}});
  $('#context-menu').addEventListener('click',e=>{const b=e.target.closest('[data-action]');if(!b)return;const id=Number(b.dataset.id),f=folders.find(x=>x.id===id);$('#context-menu').classList.add('hidden');if(b.dataset.action==='open')openFolder(id);if(b.dataset.action==='edit')folderForm(f);if(b.dataset.action==='pin'){f.pinned=!f.pinned;renderFolders();toast(f.pinned?'已固定到桌面':'已取消固定');}if(b.dataset.action==='delete'){if(confirm(`删除 FolderHub 中的「${f.name}」记录？\n仅删除应用登记，不删除磁盘真实目录。`)){folders.splice(folders.indexOf(f),1);renderFolders();toast('记录已删除，磁盘目录不受影响');}}});
  $('#modal-backdrop').addEventListener('click',e=>{if(e.target===e.currentTarget||e.target.closest('[data-close]'))closeModal();});
  document.addEventListener('click',e=>{if(!e.target.closest('#context-menu')&&!e.target.closest('.folder-more'))$('#context-menu').classList.add('hidden');if(!e.target.closest('#tray-popover')&&!e.target.closest('#tray-button'))$('#tray-popover').classList.add('hidden');});
  function syncViewModeUi(){
    grid.classList.toggle('list-mode',listMode);
    $('#grid-view-btn').classList.toggle('active',!listMode);
    $('#list-view-btn').classList.toggle('active',listMode);
  }
  $('#grid-view-btn').addEventListener('click',()=>{if(listMode){listMode=false;syncViewModeUi();toast('已切换为网格布局');}});
  $('#list-view-btn').addEventListener('click',()=>{if(!listMode){listMode=true;syncViewModeUi();toast('已切换为紧凑列表布局预览');}});
  document.addEventListener('keydown',e=>{if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='k'){e.preventDefault();$('#search-input').focus();}if(e.key==='Escape'){closeModal();$('#context-menu').classList.add('hidden');$('#tray-popover').classList.add('hidden');}});
  function tick(){const d=new Date();const t=d.toLocaleTimeString('zh-CN',{hour:'2-digit',minute:'2-digit'});$('#clock').textContent=t;$('#taskbar-clock').textContent=t;}tick();setInterval(tick,15000);
  syncThemeUi();
  applyWindowOpacity();
  renderFolders();
})();
