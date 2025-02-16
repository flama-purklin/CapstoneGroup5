import React, { useState, useEffect } from 'react';
import { AlertCircle, Check } from 'lucide-react';

const LoadingScreen = () => {
  const [llmProgress, setLLMProgress] = useState(0);
  const [characterProgress, setCharacterProgress] = useState(0);
  const [status, setStatus] = useState('Initializing...');
  const [error, setError] = useState(null);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-90 flex items-center justify-center">
      <div className="w-96 p-8 bg-gray-800 rounded-lg">
        <h2 className="text-2xl font-bold text-white mb-6">Loading Game</h2>
        
        {/* LLM Progress */}
        <div className="mb-6">
          <div className="flex justify-between text-sm text-gray-300 mb-2">
            <span>Loading Language Model</span>
            <span>{Math.round(llmProgress)}%</span>
          </div>
          <div className="w-full bg-gray-700 rounded-full h-2">
            <div 
              className="bg-blue-500 h-2 rounded-full transition-all duration-300"
              style={{ width: `${llmProgress}%` }}
            />
          </div>
        </div>

        {/* Character Progress */}
        <div className="mb-6">
          <div className="flex justify-between text-sm text-gray-300 mb-2">
            <span>Initializing Characters</span>
            <span>{Math.round(characterProgress)}%</span>
          </div>
          <div className="w-full bg-gray-700 rounded-full h-2">
            <div 
              className="bg-green-500 h-2 rounded-full transition-all duration-300"
              style={{ width: `${characterProgress}%` }}
            />
          </div>
        </div>

        {/* Status Message */}
        <div className="flex items-center gap-2 text-sm">
          {error ? (
            <AlertCircle className="w-5 h-5 text-red-500" />
          ) : llmProgress === 100 && characterProgress === 100 ? (
            <Check className="w-5 h-5 text-green-500" />
          ) : (
            <div className="w-5 h-5 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
          )}
          <span className={error ? 'text-red-500' : 'text-gray-300'}>
            {error || status}
          </span>
        </div>
      </div>
    </div>
  );
};

export default LoadingScreen;