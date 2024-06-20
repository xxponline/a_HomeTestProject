using System.Collections.Generic;
using UnityEngine;

namespace code
{
    public class DeckOfCards<T>
    {
        private List<T> _cards = new();
        private int _dealIndex = 0;
        private bool _needForceShuffle = false;

        public void PutInCard(T card)
        {
            _cards.Add(card);
            if (_cards.Count > 2)
            {
                _needForceShuffle = true;
            }
        }

        public void PutInCard(IEnumerable<T> cards)
        {
            _cards.AddRange(cards);
            if (_cards.Count > 2)
            {
                _needForceShuffle = true;
            }
        }
        
        public void Shuffle()
        {
            if (_cards.Count < 2)
            {
                return;
            }

            for (var i = 0; i < _cards.Count; ++i)
            {
                int exchangedId = Random.Range(i, _cards.Count);
                if (i != exchangedId)
                {
                    (_cards[i], _cards[exchangedId]) = (_cards[exchangedId], _cards[i]);
                }
            }

            _needForceShuffle = false;
            _dealIndex = 0;
        }

        public T Deal()
        {
            Debug.Assert(_cards.Count > 2);
            if (_needForceShuffle || _dealIndex >= _cards.Count)
            {
                Shuffle();
            }

            return _cards[_dealIndex++];
        }
    }
}