runScript('kline');

//init draw macd
ema('close',12,'emaFast');
ema('close',26,'emaSlow');
diff('emaFast','emaSlow','emaDiff');
ema('emaDiff',9,'dea');
diff('emaDiff','dea','macdDiff');

//
line('emaDiff',1,255,0,0,255,1);
line('dea',1,0,128,0,255,1);
zeroBars('macdDiff',1,0,0,255,255,1);
