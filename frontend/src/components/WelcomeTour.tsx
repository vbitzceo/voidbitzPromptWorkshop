'use client';

import { useState } from 'react';
import { X, ArrowRight } from 'lucide-react';

interface WelcomeTourProps {
  onClose: () => void;
}

export default function WelcomeTour({ onClose }: WelcomeTourProps) {
  const [currentStep, setCurrentStep] = useState(0);

  const steps = [
    {
      title: "Welcome to VoidBitz Prompt Workshop! 🎉",
      content: "Your powerful workspace for creating, managing, and executing LLM prompt templates.",
      icon: "🚀"
    },
    {
      title: "Create Beautiful Prompts 📝",
      content: "Use our intuitive editor to build prompt templates with variables, categories, and tags.",
      icon: "✨"
    },
    {
      title: "Organize with Style 📁",
      content: "Categorize prompts with custom colors and tag them for easy discovery.",
      icon: "🎨"
    },
    {
      title: "Test & Execute 🧪",
      content: "Try your prompts with real AI responses or mock data for testing.",
      icon: "⚡"
    },
    {
      title: "Import & Export 📤",
      content: "Share your prompts using YAML format - perfect for team collaboration.",
      icon: "🔄"
    }
  ];

  const nextStep = () => {
    if (currentStep < steps.length - 1) {
      setCurrentStep(currentStep + 1);
    } else {
      onClose();
    }
  };

  const skipTour = () => {
    onClose();
  };

  return (
    <div className="z-50 fixed inset-0 flex justify-center items-center bg-black bg-opacity-60 backdrop-blur-sm p-4">
      <div className="bg-white shadow-2xl rounded-2xl w-full max-w-md overflow-hidden animate-fade-in">
        {/* Progress Bar */}
        <div className="bg-gray-200 h-1">
          <div 
            className="bg-gradient-to-r from-blue-500 to-purple-500 h-1 transition-all duration-300"
            style={{ width: `${((currentStep + 1) / steps.length) * 100}%` }}
          />
        </div>

        {/* Content */}
        <div className="p-8 text-center">
          <div className="mb-4 text-6xl animate-bounce">
            {steps[currentStep].icon}
          </div>
          
          <h3 className="mb-3 font-bold text-gray-900 text-xl">
            {steps[currentStep].title}
          </h3>
          
          <p className="mb-6 text-gray-600 leading-relaxed">
            {steps[currentStep].content}
          </p>

          {/* Step indicator */}
          <div className="flex justify-center space-x-2 mb-6">
            {steps.map((_, index) => (
              <div
                key={index}
                className={`w-2 h-2 rounded-full transition-all duration-200 ${
                  index === currentStep 
                    ? 'bg-blue-500 w-6' 
                    : index < currentStep 
                      ? 'bg-green-500' 
                      : 'bg-gray-300'
                }`}
              />
            ))}
          </div>

          {/* Actions */}
          <div className="flex justify-center gap-3">
            <button
              onClick={skipTour}
              className="px-4 py-2 font-medium text-gray-600 hover:text-gray-800 transition-colors"
            >
              Skip Tour
            </button>
            <button
              onClick={nextStep}
              className="flex items-center gap-2 bg-gradient-to-r from-blue-600 hover:from-blue-700 to-purple-600 hover:to-purple-700 shadow-lg hover:shadow-xl px-6 py-2 rounded-lg font-medium text-white transition-all hover:-translate-y-0.5 duration-200 transform"
            >
              {currentStep === steps.length - 1 ? 'Get Started' : 'Next'}
              <ArrowRight className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Close button */}
        <button
          onClick={onClose}
          className="top-4 right-4 absolute p-2 text-gray-400 hover:text-gray-600 transition-colors"
        >
          <X className="w-5 h-5" />
        </button>
      </div>
    </div>
  );
}
