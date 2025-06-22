import React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { ParticipantDto } from '../types';
import ParticipantCard from './ParticipantCard';

interface DraggableParticipantCardProps {
  participant: ParticipantDto;
  onRemove?: (callSign: string) => void;
  showRemoveButton?: boolean;
  isDragging?: boolean;
}

const DraggableParticipantCard: React.FC<DraggableParticipantCardProps> = ({
  participant,
  onRemove,
  showRemoveButton,
  isDragging = false
}) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging: isSortableDragging,
  } = useSortable({
    id: participant.callSign,
  });
  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isSortableDragging ? 0.6 : 1,
    cursor: isDragging ? 'grabbing' : 'grab',
    position: isSortableDragging ? 'relative' : 'static',
    zIndex: isSortableDragging ? 1000 : 'auto',
  } as React.CSSProperties;
  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      className={`draggable-participant-card ${isSortableDragging ? 'dragging' : ''}`}
      data-testid={`draggable-participant-${participant.callSign}`}
    >
      {isSortableDragging && (
        <div className="drop-indicator" style={{
          position: 'absolute',
          top: '-2px',
          left: '0',
          right: '0',
          height: '4px',
          backgroundColor: 'var(--primary-color)',
          borderRadius: '2px',
          zIndex: 10
        }} />
      )}
      <ParticipantCard
        participant={participant}
        onRemove={onRemove}
        showRemoveButton={showRemoveButton}
      />
    </div>
  );
};

export default DraggableParticipantCard;
