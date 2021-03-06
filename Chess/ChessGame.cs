using System.Collections.Generic;
using Chessboard;
using Chessboard.Enums;

namespace ChessOnion
{
    class ChessGame
    {

        public Chessb chess { get; private set; }
        public int turn { get; private set; }
        public Color currentPlayer { get; private set; }

        public bool fineshed { get; private set; }
        private HashSet<Piece> pieces;
        private HashSet<Piece> capturedPieces;
        public bool check { get; private set; }
        public Piece vulnerableEnPassant { get; private set; }

        public ChessGame()
        {
            chess = new Chessb(8, 8);
            turn = 1;
            currentPlayer = Color.White;
            fineshed = false;
            check = false;
            vulnerableEnPassant = null;
            pieces = new HashSet<Piece>();
            capturedPieces = new HashSet<Piece>();
            setPieces();
        }

        public Piece moveMaker(Position origin, Position destiny)
        {
            Piece p = chess.removePiece(origin);
            p.increaceQtnMoves();
            Piece catchedPiece = chess.removePiece(destiny);
            chess.insertPiece(p, destiny);
            if (catchedPiece != null)
            {
                capturedPieces.Add(catchedPiece);
            }
            // # Special Move menur Roque
            if (p is King && destiny.columns == origin.columns + 2)
            {
                Position originT = new Position(origin.rows, origin.columns + 3);
                Position destinyT = new Position(origin.rows, origin.columns + 1);
                Piece T = chess.removePiece(originT);
                T.increaceQtnMoves();
                chess.insertPiece(T, destinyT);
            }
            // # Special Move Major Roque
            if (p is King && destiny.columns == origin.columns - 2)
            {
                Position originT = new Position(origin.rows, origin.columns - 4);
                Position destinyT = new Position(origin.rows, origin.columns - 1);
                Piece T = chess.removePiece(originT);
                T.increaceQtnMoves();
                chess.insertPiece(T, destinyT);
            }

            // #Special Move en Passant
            if (p is Pawn)
            {
                if (origin.columns != destiny.columns && catchedPiece == null)
                {
                    Position posP;
                    if (p.color == Color.White)
                    {
                        posP = new Position(destiny.rows + 1, destiny.columns);

                    }
                    else
                    {
                        posP = new Position(destiny.rows - 1, destiny.columns);
                    }
                    catchedPiece = chess.removePiece(posP);
                    capturedPieces.Add(catchedPiece);
                }
            }

            return catchedPiece;

        }

        public void undoMove(Position origin, Position destiny, Piece catchedPiece)
        {
            Piece p = chess.removePiece(destiny);
            p.decreaceQtnMoves();
            if (catchedPiece != null)
            {
                chess.insertPiece(catchedPiece, destiny);
                capturedPieces.Remove(catchedPiece);
            }
            chess.insertPiece(p, origin);

            // # Special Move menur Roque
            if (p is King && destiny.columns == origin.columns + 2)
            {
                Position originT = new Position(origin.rows, origin.columns + 3);
                Position destinyT = new Position(origin.rows, origin.columns + 1);
                Piece T = chess.removePiece(destinyT);
                T.decreaceQtnMoves();
                chess.insertPiece(T, originT);
            }
            // # Special Move Major Roque
            if (p is King && destiny.columns == origin.columns - 2)
            {
                Position originT = new Position(origin.rows, origin.columns - 4);
                Position destinyT = new Position(origin.rows, origin.columns - 1);
                Piece T = chess.removePiece(destinyT);
                T.decreaceQtnMoves();
                chess.insertPiece(T, originT);
            }
            // # Special Move en passant
            if (p is Pawn)
            {
                if (origin.columns != destiny.columns && catchedPiece == vulnerableEnPassant)
                {
                    Piece pawn = chess.removePiece(destiny);
                    Position posP;
                    if (pawn.color == Color.White)
                    { posP = new Position(3, destiny.columns); }
                    else
                    { posP = new Position(4, destiny.columns); }
                    chess.insertPiece(pawn, posP);
                }
            }

        }

        public void performsMove(Position origin, Position destiny)
        {
            Piece catchedPiece = moveMaker(origin, destiny);

            if (isInCheck(currentPlayer))
            {
                undoMove(origin, destiny, catchedPiece);
                throw new ChessboardException("You can't put it self in check");
            }
            if (isInCheck(Enimy(currentPlayer))) { check = true; }
            else { check = false; }

            if (checkMateTester(Enimy(currentPlayer)))
            {
                fineshed = true;
            }
            else
            {
                turn++;
                changePlayer();
            }

            Piece p = chess.piece(destiny);

            // #Special Move en Passant
            if (p is Pawn && (destiny.rows == origin.rows - 2 || destiny.rows == origin.rows + 2))
            {
                vulnerableEnPassant = p;
            }
            else { vulnerableEnPassant = null; }
        }
        private void changePlayer()
        {
            if (currentPlayer == Color.White) { currentPlayer = Color.Black; }
            else { currentPlayer = Color.White; }
        }
        public void validOriginPosition(Position pos)
        {
            if (chess.piece(pos) == null)
            {
                throw new ChessboardException("must select a piece for moves");
            }
            if (currentPlayer != chess.piece(pos).color)
            {
                throw new ChessboardException("The pice selected is not yours");
            }
            if (!chess.piece(pos).possibleMovesExist())
            {
                throw new ChessboardException("not have any moves for this piece");
            }
        }
        public void validDestinyPosition(Position origin, Position destiny)
        {
            if (!chess.piece(origin).canMoveTo(destiny))
            {
                throw new ChessboardException("Invalid destiny position");
            }
        }
        public HashSet<Piece> catchedPieces(Color color)
        {
            HashSet<Piece> aux = new HashSet<Piece>();
            foreach (Piece x in capturedPieces)
            {
                if (x.color == color)
                {
                    aux.Add(x);
                }

            }
            return aux;
        }
        public HashSet<Piece> piecesInGame(Color color)
        {
            HashSet<Piece> aux = new HashSet<Piece>();
            foreach (Piece x in pieces)
            {
                if (x.color == color)
                {
                    aux.Add(x);
                }

            }
            aux.ExceptWith(catchedPieces(color));
            return aux;
        }

        private Color Enimy(Color color)
        {
            if (color == Color.White) { return Color.Black; }
            else { return Color.White; }
        }
        private Piece King(Color color)
        {
            foreach (Piece x in piecesInGame(color))
            {
                if (x is King)
                {
                    return x;
                }

            }
            return null;
        }

        public bool isInCheck(Color color)
        {
            Piece K = King(color);
            if (K == null)
            { throw new ChessboardException($"Have not king from color {color} in chessboard"); }

            foreach (Piece x in piecesInGame(Enimy(color)))
            {
                bool[,] mat = x.possibleMoves();
                if (mat[K.position.rows, K.position.columns])
                { return true; }
            }
            return false;
        }

        public bool checkMateTester(Color color)
        {
            if (!isInCheck(color)) { return false; }

            foreach (Piece x in piecesInGame(color))
            {
                bool[,] mat = x.possibleMoves();
                for (int i = 0; i < chess.rows; i++)
                {
                    for (int j = 0; j < chess.columns; j++)
                    {
                        if (mat[i, j])
                        {
                            Position origin = x.position;
                            Position destiny = new Position(i, j);
                            Piece catchedPiece = moveMaker(origin, destiny);
                            bool testCheck = isInCheck(color);
                            undoMove(origin, destiny, catchedPiece);
                            if (!testCheck)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void setNewPiece(char column, int row, Piece piece)
        {
            chess.insertPiece(piece, new ChessPosition(column, row).toPosition());
            pieces.Add(piece);
        }
        public void setPieces()
        {

            setNewPiece('a', 1, new Tower(chess, Color.White));
            setNewPiece('h', 1, new Tower(chess, Color.White));
            setNewPiece('b', 1, new Horse(chess, Color.White));
            setNewPiece('g', 1, new Horse(chess, Color.White));
            setNewPiece('c', 1, new Bishop(chess, Color.White));
            setNewPiece('f', 1, new Bishop(chess, Color.White));
            setNewPiece('e', 1, new King(chess, Color.White, this));
            setNewPiece('d', 1, new Queen(chess, Color.White));
            setNewPiece('a', 2, new Pawn(chess, Color.White, this));
            setNewPiece('b', 2, new Pawn(chess, Color.White, this));
            setNewPiece('c', 2, new Pawn(chess, Color.White, this));
            setNewPiece('d', 2, new Pawn(chess, Color.White, this));
            setNewPiece('e', 2, new Pawn(chess, Color.White, this));
            setNewPiece('f', 2, new Pawn(chess, Color.White, this));
            setNewPiece('g', 2, new Pawn(chess, Color.White, this));
            setNewPiece('h', 2, new Pawn(chess, Color.White, this));



            setNewPiece('a', 8, new Tower(chess, Color.Black));
            setNewPiece('h', 8, new Tower(chess, Color.Black));
            setNewPiece('b', 8, new Horse(chess, Color.Black));
            setNewPiece('g', 8, new Horse(chess, Color.Black));
            setNewPiece('c', 8, new Bishop(chess, Color.Black));
            setNewPiece('f', 8, new Bishop(chess, Color.Black));
            setNewPiece('e', 8, new King(chess, Color.Black, this));
            setNewPiece('d', 8, new Queen(chess, Color.Black));
            setNewPiece('a', 7, new Pawn(chess, Color.Black, this));
            setNewPiece('b', 7, new Pawn(chess, Color.Black, this));
            setNewPiece('c', 7, new Pawn(chess, Color.Black, this));
            setNewPiece('d', 7, new Pawn(chess, Color.Black, this));
            setNewPiece('e', 7, new Pawn(chess, Color.Black, this));
            setNewPiece('f', 7, new Pawn(chess, Color.Black, this));
            setNewPiece('g', 7, new Pawn(chess, Color.Black, this));
            setNewPiece('h', 7, new Pawn(chess, Color.Black, this));


        }
    }
}