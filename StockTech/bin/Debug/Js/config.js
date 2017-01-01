//config
function config(){
	addConfig('tech','成交量');
	addConfig('tech','MACD');
	addConfig('tech','KDJ');
	
	//设置MACD为选中
	//init should run selected 
	//check('tech','MACD');
}

//init will call by host to do init draw...
function init(){
	//
	runScript('vol');
}

function onDrawItem(open,close,high,low){
	
}

//tech radio buttons click.
function onTechClick(name){
	if(name=='成交量'){
		runScript('vol');
		return;
	}
	
	if(name=='MACD'){
		runScript('macd');
		return;
	}
	
	if(name=='KDJ'){
		runScript('kdj');
		return;
	}
	
}

