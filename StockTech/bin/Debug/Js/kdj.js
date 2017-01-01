//draw top kline.
runScript('kline');

//init draw kdj
kdj('close','high','low',9,'k','d','j');
line('k',1,255,0,0,255,1);
line('d',1,0,128,0,255,1);
line('j',1,0,0,255,255,1);

