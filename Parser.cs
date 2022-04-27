
using System;



public class Parser {
	public const int _EOF = 0;
	public const int _identifier = 1;
	public const int _const = 2;
	public const int _var = 3;
	public const int _do = 4;
	public const int _while = 5;
	public const int _or = 6;
	public const int _and = 7;
	public const int _not = 8;
	public const int _number = 9;
	public const int _relOp = 10;
	public const int _eq = 11;
	public const int _ne = 12;
	public const int _assign = 13;
	public const int _add = 14;
	public const int _sub = 15;
	public const int _mul = 16;
	public const int _div = 17;
	public const int maxT = 26;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

void NoDoubleSub() {
    if (la.kind == _sub ) {
        SemErr("Unexpected double '-'.");
    }
}



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Program() {
		while (StartOf(1)) {
			Statement();
		}
	}

	void Statement() {
		if (la.kind == 2) {
			ConstInit();
			Expect(18);
		} else if (la.kind == 3) {
			VarInit();
			Expect(18);
		} else if (la.kind == 1) {
			VarAssign();
			Expect(18);
		} else if (la.kind == 4) {
			DoWhileLoop();
			Expect(18);
		} else SynErr(27);
	}

	void ConstInit() {
		Expect(2);
		Primary();
		Expect(13);
		Expect(9);
	}

	void VarInit() {
		Expect(3);
		Expect(1);
		Expect(13);
		if (StartOf(2)) {
			Expression();
		} else if (la.kind == 19) {
			ArrayInitializer();
		} else SynErr(28);
	}

	void VarAssign() {
		Primary();
		Expect(13);
		if (StartOf(2)) {
			Expression();
		} else if (la.kind == 19) {
			ArrayInitializer();
		} else SynErr(29);
	}

	void DoWhileLoop() {
		Expect(4);
		Block();
		Expect(5);
		Expect(20);
		Expression();
		Expect(21);
	}

	void Primary() {
		Expect(1);
		if (la.kind == 24) {
			Indexer();
		}
	}

	void Expression() {
		Unary();
		AssignExpression();
	}

	void ArrayInitializer() {
		Expect(19);
	}

	void Block() {
		Expect(22);
		while (StartOf(1)) {
			Statement();
		}
		Expect(23);
	}

	void Unary() {
		if (la.kind == 15) {
			Get();
			NoDoubleSub(); 
		}
		if (la.kind == 1) {
			Primary();
		} else if (la.kind == 9) {
			Get();
		} else if (la.kind == 20) {
			Get();
			Expression();
			Expect(21);
		} else if (la.kind == 8) {
			Get();
			Expression();
		} else SynErr(30);
	}

	void AssignExpression() {
		OrExpression();
		while (la.kind == 13) {
			Get();
			Unary();
			OrExpression();
		}
	}

	void OrExpression() {
		AndExpression();
		while (la.kind == 6) {
			Get();
			Unary();
			AndExpression();
		}
	}

	void AndExpression() {
		EqExpression();
		while (la.kind == 7) {
			Get();
			Unary();
			EqExpression();
		}
	}

	void EqExpression() {
		RelExpression();
		while (la.kind == 11 || la.kind == 12) {
			if (la.kind == 11) {
				Get();
			} else {
				Get();
			}
			Unary();
			RelExpression();
		}
	}

	void RelExpression() {
		AddExpression();
		while (la.kind == 10) {
			Get();
			Unary();
			AddExpression();
		}
	}

	void AddExpression() {
		MulExpression();
		while (la.kind == 14 || la.kind == 15) {
			if (la.kind == 14) {
				Get();
			} else {
				Get();
				NoDoubleSub(); 
			}
			Unary();
			MulExpression();
		}
	}

	void MulExpression() {
		while (la.kind == 16 || la.kind == 17) {
			if (la.kind == 16) {
				Get();
			} else {
				Get();
			}
			Unary();
		}
	}

	void Indexer() {
		Expect(24);
		Expression();
		Expect(25);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Program();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "identifier expected"; break;
			case 2: s = "const expected"; break;
			case 3: s = "var expected"; break;
			case 4: s = "do expected"; break;
			case 5: s = "while expected"; break;
			case 6: s = "or expected"; break;
			case 7: s = "and expected"; break;
			case 8: s = "not expected"; break;
			case 9: s = "number expected"; break;
			case 10: s = "relOp expected"; break;
			case 11: s = "eq expected"; break;
			case 12: s = "ne expected"; break;
			case 13: s = "assign expected"; break;
			case 14: s = "add expected"; break;
			case 15: s = "sub expected"; break;
			case 16: s = "mul expected"; break;
			case 17: s = "div expected"; break;
			case 18: s = "\";\" expected"; break;
			case 19: s = "\"[]\" expected"; break;
			case 20: s = "\"(\" expected"; break;
			case 21: s = "\")\" expected"; break;
			case 22: s = "\"{\" expected"; break;
			case 23: s = "\"}\" expected"; break;
			case 24: s = "\"[\" expected"; break;
			case 25: s = "\"]\" expected"; break;
			case 26: s = "??? expected"; break;
			case 27: s = "invalid Statement"; break;
			case 28: s = "invalid VarInit"; break;
			case 29: s = "invalid VarAssign"; break;
			case 30: s = "invalid Unary"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
