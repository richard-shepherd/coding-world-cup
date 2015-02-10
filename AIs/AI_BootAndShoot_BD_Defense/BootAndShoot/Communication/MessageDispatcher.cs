using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{


    public struct Telegram
    {

        public Telegram(double time, int sender, int receiver, Messages.MessageType msgType, dynamic info = null) : this()
        {
            dispatchTime = time;
            this.sender = sender;
            this.receiver = receiver;
            msg = msgType;
            this.extraInfo = info;
        }
        //the entity that sent this telegram
        private int sender;

        //the entity that is to receive this telegram
        private int receiver;

        //the message itself. These are all enumerated in the file
        //"MessageTypes.h"
        public Messages.MessageType msg { get;  set; }

        //messages can be dispatched immediately or delayed for a specified amount
        //of time. If a delay is necessary this field is stamped with the time 
        //the message should be dispatched.
        private double dispatchTime;

        //any additional information that may accompany the message
        public dynamic extraInfo { get; private set; }

        public bool IsEquals(Telegram t2)
        {
            return (Math.Abs(this.dispatchTime - t2.dispatchTime) < 0.25) &&
       (this.sender == t2.sender) &&
       (this.receiver == t2.receiver) &&
       (this.msg == t2.msg);
        }


    };

    public class MessageDispatcher
    {
        //a std::set is used as the container for the delayed messages
        //because of the benefit of automatic sorting and avoidance
        //of duplicates. Messages are sorted by their dispatch time.
       // private HashSet<Telegram> PriorityQ;


        //this method is utilized by DispatchMsg or DispatchDelayedMessages.
        //This method calls the message handling member function of the receiving
        //entity, pReceiver, with the newly created telegram
        private void Discharge(BaseGameEntity receiver, ref Telegram msg)
        {
            if (!receiver.HandleMessage(msg))
              {
                //telegram could not be handled
                  Logger.log("Message not handled", Logger.LogLevel.DEBUG);
              } 
        }

         #region Singlton 
        private static MessageDispatcher m_instance;
        public static MessageDispatcher instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MessageDispatcher();
                }
                return m_instance;
            }
        }
        private MessageDispatcher() { }
        
        #endregion


        //send a message to another agent. Receiving agent is referenced by ID.
        public void DispatchMsg(double delay,int sender,int receiver, Messages.MessageType msg ,dynamic extraInfo)
        {
            //get a pointer to the receiver
            BaseGameEntity receiverRef = EntityManager.instance.GetEntityFromID(receiver);

            //make sure the receiver is valid
            if (receiverRef == null)
            {
 
                Logger.log("\nWarning! No Receiver with ID of "  + receiver + " found",Logger.LogLevel.WARNING);
                 return;
            }
  
              //create the telegram
              Telegram telegram = new Telegram(0, sender, receiver, msg, extraInfo);
  
              //if there is no delay, route telegram immediately                       
              if (delay <= 0.0)                                                        
              {

                  Logger.log("Telegram dispatched by " + sender +
                      " for " + receiver + ". Msg is " + msg + "", Logger.LogLevel.INFO);

                //send the telegram to the recipient
                Discharge(receiverRef, ref telegram);
              }

  //else calculate the time when the telegram should be dispatched
  //else
  //{
  //  double CurrentTime = TickCounter->GetCurrentFrame(); 

  //  telegram.DispatchTime = CurrentTime + delay;

  //  //and put it in the queue
  //  PriorityQ.insert(telegram);   

  //  #ifdef SHOW_MESSAGING_INFO
  //  debug_con << "\nDelayed telegram from " << sender << " recorded at time " 
  //          << TickCounter->GetCurrentFrame() << " for " << receiver
  //          << ". Msg is " << msg << "";
  //  #endif
  //}
        }

        //send out any delayed messages. This method is called each time through   
        //the main game loop.
        public void DispatchDelayedMessages()
        {

        }
    }
}
