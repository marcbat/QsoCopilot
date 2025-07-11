import React, { useState } from 'react';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
  DragStartEvent,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  rectSortingStrategy,
} from '@dnd-kit/sortable';
import {
  restrictToParentElement,
} from '@dnd-kit/modifiers';
import { ParticipantDto } from '../types';
import DraggableParticipantCard from './DraggableParticipantCard';

interface DraggableParticipantsListProps {
  participants: ParticipantDto[];
  onReorder: (participants: ParticipantDto[]) => void;
  onRemove?: (callSign: string) => void;
  showRemoveButton?: boolean;
  isReordering?: boolean;
  shouldFetchQrzInfo?: boolean;
}

const DraggableParticipantsList: React.FC<DraggableParticipantsListProps> = ({
  participants,
  onReorder,
  onRemove,
  showRemoveButton = false,
  isReordering = false,
  shouldFetchQrzInfo = false
}) => {
  const [isDragActive, setIsDragActive] = useState(false);
  
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Trier les participants par ordre pour l'affichage
  const sortedParticipants = [...participants].sort((a, b) => a.order - b.order);

  const handleDragStart = (_event: DragStartEvent) => {
    setIsDragActive(true);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    setIsDragActive(false);

    if (active.id !== over?.id) {
      const oldIndex = sortedParticipants.findIndex((p) => p.callSign === active.id);
      const newIndex = sortedParticipants.findIndex((p) => p.callSign === over?.id);

      const reorderedParticipants = arrayMove(sortedParticipants, oldIndex, newIndex);
      
      // Recalculer les ordres
      const participantsWithNewOrder = reorderedParticipants.map((participant, index) => ({
        ...participant,
        order: index + 1
      }));

      onReorder(participantsWithNewOrder);
    }
  };

  return (
    <div className="draggable-participants-list">
      {isReordering && (
        <div className="reordering-info" style={{
          background: 'var(--info-bg, #e1f5fe)',
          color: 'var(--info-text, #01579b)',
          padding: '0.75rem',
          borderRadius: '4px',
          marginBottom: '1rem',
          fontSize: '0.875rem',
          border: '1px solid var(--info-border, #81d4fa)'
        }}>
          <strong>Réorganisation en cours...</strong> Les participants seront réorganisés automatiquement.
        </div>
      )}
      
      {!isReordering && sortedParticipants.length > 1 && (
        <div className="drag-help-info" style={{
          background: 'var(--background-color)',
          color: 'var(--text-secondary)',
          padding: '0.75rem',
          borderRadius: '4px',
          marginBottom: '1rem',
          fontSize: '0.85rem',
          border: '1px solid var(--border-color)',
          display: 'flex',
          alignItems: 'center',
          gap: '0.5rem'
        }}>
          <span>🔄</span>
          <span>Survolez une carte participant et glissez-déposez pour réorganiser l'ordre</span>
        </div>
      )}
      
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
        modifiers={[restrictToParentElement]}
      >
        <SortableContext
          items={sortedParticipants.map(p => p.callSign)}
          strategy={rectSortingStrategy}
        >          <div className={`participants-grid ${isDragActive ? 'drag-active' : ''}`} style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
            gap: '1rem',
            alignItems: 'stretch', /* Force toutes les cartes d'une ligne à avoir la même hauteur */
            width: '100%',
          }}>            {sortedParticipants.map((participant) => (
              <DraggableParticipantCard
                key={participant.callSign}
                participant={participant}
                onRemove={onRemove}
                showRemoveButton={showRemoveButton}
                isDragging={isReordering}
                shouldFetchQrzInfo={shouldFetchQrzInfo}
              />
            ))}
          </div>
        </SortableContext>
      </DndContext>
      
      {sortedParticipants.length === 0 && (
        <div className="no-participants" style={{
          textAlign: 'center',
          color: 'var(--text-secondary)',
          padding: '2rem',
          fontStyle: 'italic'
        }}>
          Aucun participant ajouté pour le moment
        </div>
      )}
    </div>
  );
};

export default DraggableParticipantsList;
