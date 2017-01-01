#config our stock system.
#stk is core stock object in our system.

def drawVol():
	stk.clearDrawings();
	stk.candleLine('price_candle',0,'open','close','high','low');
	#vol graph bottom
	stk.vLines('vol_part',1,'vol');
	stk.refreshCanvas();
	
def drawMACD():
	stk.clearDrawings();
	#draw kline 
	stk.candleLine('price_candle',0,'open','close','high','low');
	
	#init draw macd
	stk.ema('close',12,'emaFast');
	stk.ema('close',26,'emaSlow');
	stk.diff('emaFast','emaSlow','emaDiff');
	stk.ema('emaDiff',9,'dea');
	stk.diff('emaDiff','dea','macdDiff');

	#draw macd.
	#line is add a line to canvas.
	stk.line('emaDiff',1,255,0,0,255,1);
	stk.line('dea',1,0,128,0,255,1);
	stk.zeroBars('macdDiff',1,0,0,255,255,1);
	stk.refreshCanvas();
	
def drawKDJ():
	stk.clearDrawings();
	#draw kline 
	stk.candleLine('price_candle',0,'open','close','high','low');
	
	#draw kdj
	stk.kdj('close','high','low',9,'k','d','j');
	stk.line('k',1,255,0,0,255,1);
	stk.line('d',1,0,128,0,255,1);
	stk.line('j',1,0,0,255,255,1);
	stk.refreshCanvas();
	
def drawRSI():
	stk.clearDrawings();
	stk.candleLine('price_candle',0,'open','close','high','low');
	
	#formula construct...
	stk.formula();
	stk.addFormula("lc:close[-1]");
	stk.addFormula("rsi=sma(max(close-lc,0),14)/sma(abs(close-lc),14)*100");
	
	stk.formulaEnd();
	
	#hard coded for now.
	stk.rsi();
	
	#stk.
	stk.refreshCanvas();
	
def drawEMADiffs():
	stk.clearDrawings();
	stk.candleLine('price_candle',0,'open','close','high','low');
	
	#addEMALine(1);
	stk.ema('close',5,'ema5');
	stk.ema('close',10,'ema10');
	stk.ema('close',20,'ema20');
	stk.ema('close',30,'ema30');
	stk.diff('ema5','ema10','emaDiff 5-10');
	stk.diff('ema5','ema20','emaDiff 5-20');
	stk.diff('ema5','ema30','emaDiff 5-30');
	
	

	#draw macd.
	#line is add a line to canvas.
	stk.line('emaDiff 5-10',1,255,0,0,255,1);
	stk.line('emaDiff 5-20',1,0,255,0,255,1);
	stk.line('emaDiff 5-30',1,0,0,255,255,1);
	
	stk.refreshCanvas();
	
def config():
	stk.set("tech","成交量");
	stk.set("tech","MACD");
	stk.set("tech","KDJ");
	#stk.set("tech","RSI");
	#stk.set("tech","数据");
	stk.set("tech","均线之差");
	
	return 0;
def databox():
	stk.showDownloadBox();
	
#our host will call it.
def onTechChecked(name):
	if name=='成交量':
		drawVol();
		return 1;
	if name=='MACD':
		drawMACD();
		return 2;
	if name=='KDJ':
		drawKDJ();
		return 3;
	if name=='RSI':
		drawRSI();
		return 4;
	if name=='数据':
		databox();
		return 4;
	if name=='均线之差':
		drawEMADiffs();
		return 4;
		
	return 0;
	

def contextMenu():
	stk.addContextMenu("5日均线");
	stk.addContextMenu("10日均线");
	stk.addContextMenu("20日均线");
	
	
def addEMALine(days):
	id='ema'+str(days);
	stk.ema('close',days,id);
	stk.line(id,0,0,0,255,255,4);
	stk.refreshCanvas();
	
def onContextMenu(name):
	if name=="5日均线":
		addEMALine(5);
		return 0;
	if name=="10日均线":
		addEMALine(10);
		return 0;
	if name=="20日均线":
		addEMALine(20);
		return 0;
		
	return 0;
	
	
config();
contextMenu();
drawVol();
