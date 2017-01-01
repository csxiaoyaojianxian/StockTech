#init our system.
url="http://www.sunshinestudio.com";
#debug
#url="http://localhost";

stockUrl=url+"/stockdata";


def init():
	stk.setStr("stockUrl",stockUrl);
	
init();
